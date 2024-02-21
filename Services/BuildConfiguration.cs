using System.Reflection;

namespace Rwb.Images.Services
{
    public class BuildConfiguration
    {
        private bool _Paid;
        public bool Paid {  get {  return _Paid; } }    

        public BuildConfiguration()
        {
            AssemblyConfigurationAttribute assemblyConfigurationAttribute = GetType().Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            string buildConfigurationName = assemblyConfigurationAttribute?.Configuration;
            _Paid = buildConfigurationName.Contains("(paid)");
        }

    }
}
