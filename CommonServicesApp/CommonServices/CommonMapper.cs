using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonServices
{
    public static class CommonMapper
    {

        private static TypeConverter GetTypeConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }

        private static object? ConvertToTarget(Type srcType, Type destType, object? sourceObj, bool isProtoObj = true)
        {
            if (srcType.Name == typeof(string).Name && string.IsNullOrWhiteSpace(Convert.ToString(sourceObj)))
            {
                return null;
            }

            TypeConverter typeConverter = GetTypeConverter(srcType);

            if (typeConverter.CanConvertTo(destType))
            {
                if (!srcType.FullName.Contains("DateTime"))
                {
                    return typeConverter.ConvertTo(srcType, destType);
                }

                try
                {
                    return typeConverter.ConvertTo(srcType, destType);
                }
                catch (Exception)
                {
                    if (srcType != null)
                    {
                        object dateTimeConversion = GetDateTimeConversion(srcType, isProtoObj);
                        return typeConverter.ConvertFrom(dateTimeConversion);
                    }
                }
            }
            return null;
        }

        private static object GetDateTimeConversion(object SourceObj, bool IsProtoToCllObj = true)
        {

            DateTime dateTime = DateTime.MinValue;
            List<string> list = new List<string>();

            if (SourceObj != null)
            {
                list.AddRange(CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns().ToList());
            }

            try
            {
                if (IsProtoToCllObj)
                {
                    dateTime = Convert.ToDateTime(DateTime.ParseExact(Convert.ToString(SourceObj), list.ToArray(), CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern));
                    return dateTime;
                }
            }
            catch (Exception)
            {
                return SourceObj;
            }

            return null;

        }

        public static void GetClassObj<T1, T2>(T1 protoObj, T2 ClassObj)
        {
            if (protoObj == null || ClassObj == null)
            {
                return;
            }

            PropertyInfo[] propertyInfos = protoObj.GetType().GetProperties();
            PropertyInfo[] propertyInfos1 = ClassObj.GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos1)
            {
                PropertyInfo property = propertyInfos.Where((PropertyInfo x) => x.Name.Replace("_", "").ToLower() == propertyInfo.Name.Replace("_", "").ToLower()).FirstOrDefault();

                property.SetValue(ClassObj, ConvertToTarget(propertyInfo.PropertyType, property.PropertyType, property.GetValue(protoObj)));
            }
        }


        public static void SetProto<T1, T2>(T1 ClassProp2, T2 protoObj)
        {
            if (protoObj == null || ClassProp2 == null)
            {
                return;
            }

            PropertyInfo[] protoInfos = protoObj.GetType().GetProperties();
            PropertyInfo[] classInfos = ClassProp2.GetType().GetProperties();

            foreach (PropertyInfo protInfo in protoInfos)
            {
                PropertyInfo propertyInfo = classInfos.Where((PropertyInfo x) => x.Name.Replace("_", "").ToLower() == protInfo.Name.Replace("_", "").ToLower()).FirstOrDefault();
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    object obj = ConvertToTarget(propertyInfo.PropertyType, protInfo.PropertyType, propertyInfo.GetValue(classInfos), isProtoObj: true);
                    propertyInfo.SetValue(protoObj, obj);
                }
            }


        }
    }
}
