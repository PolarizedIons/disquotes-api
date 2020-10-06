using System.Linq;
using System.Reflection;

namespace QuotesApi.Extentions
{
    public static class ObjectExtention
    {
        public static T MapProps<T>(this T target, object from)
        {
            var targetFields = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy ).Where(x => x.CanWrite);
            var fromProps = from.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy ).Where(x => x.CanRead);
            var fromFieldsNames = fromProps.Select(x => x.Name);
            var overlap = targetFields.Where(field => fromFieldsNames.Contains(field.Name));

            foreach (var fieldInfo in overlap)
            {
                var value = fromProps.First(x => x.Name == fieldInfo.Name).GetValue(from);
                fieldInfo.SetValue(target, value);
            }

            return target;
        }
    }
}
