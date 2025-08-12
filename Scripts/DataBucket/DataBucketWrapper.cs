using System;
using System.Collections.Generic;
using Firebase.Analytics;

namespace JacatGames.Tracking
{
    public struct ParameterData
    {
        public string Key { get; }
        public object Value { get; }
        public Type ValueType { get; }

        // Constructor to initialize the parameter with a key and value
        public ParameterData(string key, object value)
        {
            Key = key;
            Value = value;
            if (value != null)
            {
                ValueType = value.GetType();
            }
            else
            {
                ValueType = typeof(string);
            }
        }

        // Convert to Firebase Analytics Parameter
        public Parameter ToFirebaseParameter()
        {
            if (Value is int intValue)
            {
                return new Parameter(Key, intValue);
            }

            if (Value is double doubleValue)
            {
                return new Parameter(Key, doubleValue);
            }

            if (Value is float floatValue) // Handle float as double
            {
                return new Parameter(Key, floatValue);
            }

            if (Value is string stringValue)
            {
                return new Parameter(Key, stringValue);
            }

            if (Value is null)
            {
                return new Parameter(Key, (string)null);
            }

            // Optionally, handle unsupported types
            throw new InvalidOperationException($"Unsupported type for tracking parameter: {ValueType}");
        }
    }

    public partial class DataBucketWrapper
    {
        public static void LogEvent(string name, string paramName, int value)
        {
            #if DATABUCKET_REALTIME
            DataBucketUploader.LogEvent(name, paramName, value);
            #endif
            FirebaseManager.LogEvent(name, paramName, value);
        }

        public static void LogEvent(string name, string paramName, double value)
        {
            #if DATABUCKET_REALTIME
            DataBucketUploader.LogEvent(name, paramName, value);
            #endif
            FirebaseManager.LogEvent(name, paramName, value);
        }

        public static void LogEvent(string name, string paramName, string value)
        {
            #if DATABUCKET_REALTIME
            DataBucketUploader.LogEvent(name, paramName, value);
            #endif
            FirebaseManager.LogEvent(name, paramName, value);
        }

        public static void LogEvent(string name)
        {
            #if DATABUCKET_REALTIME
            DataBucketUploader.LogEvent(name);
            #endif
            FirebaseManager.LogEvent(name);
        }

        public static void LogEvent(string name, params ParameterData[] parameters)
        {
            // Convert ParameterData to Firebase Analytics Parameters
            List<Firebase.Analytics.Parameter> firebaseParameters = new List<Firebase.Analytics.Parameter>();
            foreach (var param in parameters)
            {
                firebaseParameters.Add(param.ToFirebaseParameter());
            }

            FirebaseManager.LogEvent(name, firebaseParameters.ToArray());

            #if DATABUCKET_REALTIME
            DataBucketUploader.LogEvent(name, parameters);
            #endif
        }
        
        public static void SetUserProperties(string name, string property)
        {
            #if DATABUCKET_REALTIME
            DataBucketUploader.SetUserProperties(name, property);
            #endif
            FirebaseManager.SetUserProperties(name, property);
        }
    }
}