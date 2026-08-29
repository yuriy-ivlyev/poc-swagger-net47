using System;

namespace PocSwagger.Attributes
{
    /// <summary>
    /// Specifies which API consumers can access the decorated action or controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ApiConsumerAttribute : Attribute
    {
        public string[] Consumers { get; }

        public ApiConsumerAttribute(params string[] consumers)
        {
            if (consumers == null || consumers.Length == 0)
                throw new ArgumentException("At least one consumer must be specified.", nameof(consumers));

            Consumers = consumers;
        }
    }
}
