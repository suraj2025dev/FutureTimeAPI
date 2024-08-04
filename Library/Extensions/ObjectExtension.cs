using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Extensions
{
   public static class ObjectExtension
    {
        
        public static void TrimStringProperties<T>(this List<T> items)
        {
            foreach(var item in items){
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var p in properties)
                {
                    if (p.PropertyType != typeof(string) || !p.CanWrite || !p.CanRead) { continue; }
                    var value = p.GetValue(item) as string;
                    try{
                        p.SetValue(item,value.Trim());
                    }catch(Exception ex){
                        
                    }
                    
                }
            }
            
        }

        public static void TrimStringProperty<T>(this T item)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in properties)
            {
                if (p.PropertyType != typeof(string) || !p.CanWrite || !p.CanRead) { continue; }
                var value = p.GetValue(item) as string;
                try{
                    p.SetValue(item,value.Trim());
                }catch(Exception ex){
                    
                }
                
            }
        }
    }
}
