using System.Linq;
using System.Reflection;
using Serilog;

namespace QuotesApi.Extentions
{
    public static class ObjectExtention
    {
        public static T MapPropsTo<T>(this object from, T target)
        {
            Log.Debug("Mapping from {from} to {to}", from.GetType(), typeof(T));
            var targetFields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy ).Where(x => x.CanWrite);
            var fromFields = from.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy ).Where(x => x.CanRead);
            var fromFieldsNames = fromFields.Select(x => x.Name);
            var overlap = targetFields.Where(field => fromFieldsNames.Contains(field.Name));

            foreach (var fieldInfo in overlap)
            {
                var value = fromFields.First(x => x.Name == fieldInfo.Name).GetValue(from);
                fieldInfo.SetValue(target, value);
            }

            
            Log.Debug("Returning {type} {@target}", target.GetType(), target);
            return target;
        }
    }
}
