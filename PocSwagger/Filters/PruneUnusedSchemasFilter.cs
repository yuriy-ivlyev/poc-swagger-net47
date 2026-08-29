using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger;

namespace PocSwagger.Filters
{
    /// <summary>
    /// Removes from the <c>definitions</c> section any schema that is not
    /// referenced by any operation in the current document.
    /// </summary>
    public class PruneUnusedSchemasFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, System.Web.Http.Description.IApiExplorer apiExplorer)
        {
            if (swaggerDoc.definitions == null || swaggerDoc.definitions.Count == 0)
                return;

            var usedRefs = new HashSet<string>();
            CollectRefsFromPaths(swaggerDoc.paths, usedRefs);

            // Iteratively resolve $ref chains inside definitions themselves
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var key in usedRefs.ToList())
                {
                    if (swaggerDoc.definitions.TryGetValue(key, out Schema schema))
                    {
                        int before = usedRefs.Count;
                        CollectRefsFromSchema(schema, usedRefs);
                        if (usedRefs.Count != before)
                            changed = true;
                    }
                }
            }

            var unused = swaggerDoc.definitions.Keys
                .Where(k => !usedRefs.Contains(k))
                .ToList();

            foreach (var key in unused)
                swaggerDoc.definitions.Remove(key);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void CollectRefsFromPaths(
            IDictionary<string, PathItem> paths,
            ISet<string> refs)
        {
            if (paths == null) return;

            foreach (var path in paths.Values)
            {
                foreach (var op in GetOperations(path))
                {
                    CollectRefsFromOperation(op, refs);
                }
            }
        }

        private static IEnumerable<Operation> GetOperations(PathItem path)
        {
            var ops = new List<Operation>();
            if (path.get != null)    ops.Add(path.get);
            if (path.post != null)   ops.Add(path.post);
            if (path.put != null)    ops.Add(path.put);
            if (path.patch != null)  ops.Add(path.patch);
            if (path.delete != null) ops.Add(path.delete);
            if (path.head != null)   ops.Add(path.head);
            if (path.options != null) ops.Add(path.options);
            return ops;
        }

        private static void CollectRefsFromOperation(Operation op, ISet<string> refs)
        {
            if (op.parameters != null)
            {
                foreach (var p in op.parameters)
                    CollectRefsFromSchema(p.schema, refs);
            }

            if (op.responses != null)
            {
                foreach (var r in op.responses.Values)
                    CollectRefsFromSchema(r.schema, refs);
            }
        }

        private static void CollectRefsFromSchema(Schema schema, ISet<string> refs)
        {
            if (schema == null) return;

            if (schema.@ref != null)
            {
                // Only handle local definition references in the format "#/definitions/SomeName"
                const string definitionsPrefix = "#/definitions/";
                if (schema.@ref.StartsWith(definitionsPrefix, StringComparison.Ordinal))
                {
                    var name = schema.@ref.Substring(definitionsPrefix.Length);
                    refs.Add(name);
                }
                return;
            }

            CollectRefsFromSchema(schema.items, refs);

            if (schema.properties != null)
                foreach (var p in schema.properties.Values)
                    CollectRefsFromSchema(p, refs);

            if (schema.additionalProperties != null)
                CollectRefsFromSchema(schema.additionalProperties, refs);

            if (schema.allOf != null)
                foreach (var s in schema.allOf)
                    CollectRefsFromSchema(s, refs);
        }
    }
}
