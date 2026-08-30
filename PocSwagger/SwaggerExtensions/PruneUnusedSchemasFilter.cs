using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace PocSwagger.SwaggerExtensions
{
    public class PruneUnusedSchemasFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            if (swaggerDoc == null || swaggerDoc.definitions == null || swaggerDoc.definitions.Count == 0)
                return;

            var reachableDefinitions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visitedSchemas = new HashSet<Schema>(ReferenceEqualityComparer<Schema>.Instance);

            foreach (var pathItem in (swaggerDoc.paths ?? new Dictionary<string, PathItem>()).Values)
                CollectFromPathItem(pathItem, reachableDefinitions, visitedSchemas);

            foreach (var parameter in (swaggerDoc.parameters ?? new Dictionary<string, Parameter>()).Values)
                CollectFromParameter(parameter, reachableDefinitions, visitedSchemas);

            foreach (var response in (swaggerDoc.responses ?? new Dictionary<string, Response>()).Values)
                CollectFromResponse(response, reachableDefinitions, visitedSchemas);

            var queue = new Queue<string>(reachableDefinitions);
            while (queue.Count > 0)
            {
                var definitionName = queue.Dequeue();
                Schema definitionSchema;
                if (!swaggerDoc.definitions.TryGetValue(definitionName, out definitionSchema))
                    continue;

                foreach (var nestedRef in CollectReferencedDefinitions(definitionSchema))
                {
                    if (reachableDefinitions.Add(nestedRef))
                        queue.Enqueue(nestedRef);
                }
            }

            var unusedDefinitions = swaggerDoc.definitions.Keys
                .Where(definitionName => !reachableDefinitions.Contains(definitionName))
                .ToList();

            foreach (var definitionName in unusedDefinitions)
                swaggerDoc.definitions.Remove(definitionName);

            // Safety check: if anything remains unreachable, fail fast so it cannot be exported.
            var remainingOrphans = FindOrphanDefinitions(swaggerDoc);
            if (remainingOrphans.Count > 0)
            {
                throw new InvalidOperationException(
                    "Orphan Swagger definitions detected after pruning: " + string.Join(", ", remainingOrphans));
            }
        }

        private static List<string> FindOrphanDefinitions(SwaggerDocument swaggerDoc)
        {
            var reachableDefinitions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visitedSchemas = new HashSet<Schema>(ReferenceEqualityComparer<Schema>.Instance);

            foreach (var pathItem in (swaggerDoc.paths ?? new Dictionary<string, PathItem>()).Values)
                CollectFromPathItem(pathItem, reachableDefinitions, visitedSchemas);

            foreach (var parameter in (swaggerDoc.parameters ?? new Dictionary<string, Parameter>()).Values)
                CollectFromParameter(parameter, reachableDefinitions, visitedSchemas);

            foreach (var response in (swaggerDoc.responses ?? new Dictionary<string, Response>()).Values)
                CollectFromResponse(response, reachableDefinitions, visitedSchemas);

            var queue = new Queue<string>(reachableDefinitions);
            while (queue.Count > 0)
            {
                var definitionName = queue.Dequeue();
                Schema definitionSchema;
                if (!swaggerDoc.definitions.TryGetValue(definitionName, out definitionSchema))
                    continue;

                foreach (var nestedRef in CollectReferencedDefinitions(definitionSchema))
                {
                    if (reachableDefinitions.Add(nestedRef))
                        queue.Enqueue(nestedRef);
                }
            }

            return swaggerDoc.definitions.Keys
                .Where(definitionName => !reachableDefinitions.Contains(definitionName))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void CollectFromPathItem(PathItem pathItem, ISet<string> reachableDefinitions, ISet<Schema> visitedSchemas)
        {
            if (pathItem == null)
                return;

            if (pathItem.parameters != null)
            {
                foreach (var parameter in pathItem.parameters)
                    CollectFromParameter(parameter, reachableDefinitions, visitedSchemas);
            }

            CollectFromOperation(pathItem.get, reachableDefinitions, visitedSchemas);
            CollectFromOperation(pathItem.put, reachableDefinitions, visitedSchemas);
            CollectFromOperation(pathItem.post, reachableDefinitions, visitedSchemas);
            CollectFromOperation(pathItem.delete, reachableDefinitions, visitedSchemas);
            CollectFromOperation(pathItem.options, reachableDefinitions, visitedSchemas);
            CollectFromOperation(pathItem.head, reachableDefinitions, visitedSchemas);
            CollectFromOperation(pathItem.patch, reachableDefinitions, visitedSchemas);
        }

        private static void CollectFromOperation(Operation operation, ISet<string> reachableDefinitions, ISet<Schema> visitedSchemas)
        {
            if (operation == null)
                return;

            if (operation.parameters != null)
            {
                foreach (var parameter in operation.parameters)
                    CollectFromParameter(parameter, reachableDefinitions, visitedSchemas);
            }

            if (operation.responses != null)
            {
                foreach (var response in operation.responses.Values)
                    CollectFromResponse(response, reachableDefinitions, visitedSchemas);
            }
        }

        private static void CollectFromParameter(Parameter parameter, ISet<string> reachableDefinitions, ISet<Schema> visitedSchemas)
        {
            if (parameter == null)
                return;

            CollectFromPartialSchema(parameter, reachableDefinitions, visitedSchemas);

            if (!string.IsNullOrEmpty(parameter.@ref))
            {
                var definitionName = ExtractDefinitionName(parameter.@ref);
                if (!string.IsNullOrEmpty(definitionName))
                    reachableDefinitions.Add(definitionName);
            }

            CollectFromSchema(parameter.schema, reachableDefinitions, visitedSchemas);
        }

        private static void CollectFromResponse(Response response, ISet<string> reachableDefinitions, ISet<Schema> visitedSchemas)
        {
            if (response == null)
                return;

            CollectFromSchema(response.schema, reachableDefinitions, visitedSchemas);
        }

        private static void CollectFromPartialSchema(PartialSchema partialSchema, ISet<string> reachableDefinitions, ISet<Schema> visitedSchemas)
        {
            if (partialSchema == null)
                return;

            CollectFromPartialSchema(partialSchema.items, reachableDefinitions, visitedSchemas);
        }

        private static void CollectFromSchema(Schema schema, ISet<string> reachableDefinitions, ISet<Schema> visitedSchemas)
        {
            if (schema == null || !visitedSchemas.Add(schema))
                return;

            foreach (var definitionName in CollectReferencedDefinitions(schema))
                reachableDefinitions.Add(definitionName);

            if (schema.allOf != null)
            {
                foreach (var childSchema in schema.allOf)
                    CollectFromSchema(childSchema, reachableDefinitions, visitedSchemas);
            }

            if (schema.properties != null)
            {
                foreach (var childSchema in schema.properties.Values)
                    CollectFromSchema(childSchema, reachableDefinitions, visitedSchemas);
            }

            CollectFromSchema(schema.additionalProperties, reachableDefinitions, visitedSchemas);
        }

        private static IEnumerable<string> CollectReferencedDefinitions(Schema schema)
        {
            if (schema == null)
                yield break;

            if (!string.IsNullOrEmpty(schema.@ref))
            {
                var definitionName = ExtractDefinitionName(schema.@ref);
                if (!string.IsNullOrEmpty(definitionName))
                    yield return definitionName;

                yield break;
            }

            if (schema.items != null)
            {
                foreach (var definitionName in CollectReferencedDefinitions(schema.items))
                    yield return definitionName;
            }

            if (schema.allOf != null)
            {
                foreach (var childSchema in schema.allOf)
                {
                    foreach (var definitionName in CollectReferencedDefinitions(childSchema))
                        yield return definitionName;
                }
            }

            if (schema.properties != null)
            {
                foreach (var childSchema in schema.properties.Values)
                {
                    foreach (var definitionName in CollectReferencedDefinitions(childSchema))
                        yield return definitionName;
                }
            }

            if (schema.additionalProperties != null)
            {
                foreach (var definitionName in CollectReferencedDefinitions(schema.additionalProperties))
                    yield return definitionName;
            }
        }

        private static string ExtractDefinitionName(string reference)
        {
            const string prefix = "#/definitions/";
            if (reference.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return reference.Substring(prefix.Length);

            return null;
        }

        private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

            public bool Equals(T x, T y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}