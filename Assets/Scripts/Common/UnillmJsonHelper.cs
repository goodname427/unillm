using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.VisualScripting.FullSerializer;

namespace unillm
{
    public static class UnillmJsonHelper
    {
        /// <summary>
        /// 获得标准的Json字符串，去掉多余的字符
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string GetCleanJson(string json)
        {
            string pattern = @"^```json\s*|\s*```$";

            // 过滤可能的代码包围块
            string cleanJson = Regex.Replace(json, pattern, "", RegexOptions.Multiline);

            return cleanJson.Trim();
        }

        /// <summary>
        /// JsonData转Object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ParseDataToObject(fsData data, Type type)
        {
            if (data.IsNull)
            {
                return null;
            }

            if (data.IsBool && type == typeof(bool))
            {
                return data.AsBool;
            }

            if (data.IsDouble && (type == typeof(double) || type == typeof(float)))
            {
                return Convert.ChangeType(data.AsDouble, type);
            }

            if (data.IsInt64 && (type == typeof(long) || type == typeof(int)))
            {
                return Convert.ChangeType(data.AsInt64, type);
            }

            if (data.IsString && type == typeof(string))
            {
                return data.AsString;
            }

            if (data.IsString && typeof(Enum).IsAssignableFrom(type))
            {
                if (Enum.TryParse(type, data.AsString, true, out var result))
                {
                    return result;
                }
            }

            if (data.IsList && typeof(IList).IsAssignableFrom(type))
            {
                // 处理泛型列表（如 List<T>）
                if (type.IsGenericType)
                {
                    var elementType = type.GetGenericArguments()[0];

                    var listObj = (IList)Activator.CreateInstance(type);
                    if (listObj is null)
                    {
                        UnillmLogger.Error($"Try Create Instance of {type} Failed");
                        return null;
                    }
                    
                    foreach (var itemData in data.AsList)
                    {
                        listObj.Add(ParseDataToObject(itemData, elementType));
                    }

                    return listObj;
                }
                // 处理数组（如 T[]）
                else if (type.IsArray)
                {
                    var elementType = type.GetElementType();

                    var listObj = Array.CreateInstance(elementType, data.AsList.Count);
                    if (listObj is null)
                    {
                        UnillmLogger.Error($"Try Create Instance of {type} Failed");
                        return null;
                    }

                    int index = 0;
                    foreach (var itemData in data.AsList)
                    {
                        listObj.SetValue(ParseDataToObject(itemData, elementType), index++);
                    }

                    return listObj;
                }
                else
                {
                    UnillmLogger.Error($"Unsupported list type: {type}");
                    return null;
                }
            }

            if (data.IsDictionary)
            {
                if (Activator.CreateInstance(type) is not object obj)
                {
                    UnillmLogger.Error($"Try Create Instance of {type} Failed");
                    return null;
                }

                var dicData = data.AsDictionary;

                foreach (var field in type.GetFields())
                {
                    if (dicData.TryGetValue(field.Name, out var itemData))
                    {
                        field.SetValue(obj, ParseDataToObject(itemData, field.FieldType));
                    }
                }

                foreach (var property in type.GetProperties())
                {
                    if (property.CanWrite && dicData.TryGetValue(property.Name, out var itemData))
                    {
                        property.SetValue(obj, ParseDataToObject(itemData, property.PropertyType));
                    }
                }

                return obj;
            }

            return null;
        }

        /// <summary>
        /// Json字符串转Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json, Type type = null) where T : new()
        {
            var data = fsJsonParser.Parse(json);

            type ??= typeof(T);
            return (T)ParseDataToObject(data, type);
        }

        /// <summary>
        /// Object转JsonData
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static fsData ParseObjectToData(object obj)
        {
            if (obj is null)
            {
                return fsData.Null;
            }

            if (obj is bool boolObj)
            {
                return new fsData(boolObj);
            }

            if (obj is float floatObj)
            {
                return new fsData(floatObj);
            }

            if (obj is double doubleObj)
            {
                return new fsData(doubleObj);
            }

            if (obj is int intObj)
            {
                return new fsData(intObj);
            }

            if (obj is long longObj)
            {
                return new fsData(longObj);
            }

            if (obj is string stringObj)
            {
                return new fsData(stringObj);
            }

            if (obj is Enum)
            {
                return new fsData(Enum.GetName(obj.GetType(), obj));
            }

            if (obj is IList listObj)
            {
                var listData = fsData.CreateList(listObj.Count);

                foreach (var itemObj in listObj)
                {
                    listData.AsList.Add(ParseObjectToData(itemObj));
                }

                return listData;
            }

            var type = obj.GetType();
            var dicData = fsData.CreateDictionary();

            foreach (var field in type.GetFields())
            {
                dicData.AsDictionary.Add(field.Name, ParseObjectToData(field.GetValue(obj)));
            }

            foreach (var property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    dicData.AsDictionary.Add(property.Name, ParseObjectToData(property.GetValue(obj)));
                }
            }

            return dicData;
        }

        /// <summary>
        /// Object转Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(object obj)
        {
            fsData fsData = ParseObjectToData(obj);

            return fsJsonPrinter.PrettyJson(fsData);
        }
    }
}
