using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCharts.Data
{
    public class RawData
    {
        public DateTime Timestamp { get; set; } // Timestamp for the record

        // Dictionary to hold dynamic fields
        public Dictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

        // Method to add or update a dynamic field
        public void AddOrUpdateField(string fieldName, object value)
        {
            if (Fields.ContainsKey(fieldName))
            {
                Fields[fieldName] = value;
            }
            else
            {
                Fields.Add(fieldName, value);
            }
        }

        // Method to retrieve a dynamic field value
        public object GetFieldValue(string fieldName)
        {
            return Fields.TryGetValue(fieldName, out var value) ? value : null;
        }

        // Override ToString() for easy debugging
        public override string ToString()
        {
            var fieldsString = string.Join(", ", Fields.Select(kv => $"{kv.Key}: {kv.Value}"));
            return $"Timestamp: {Timestamp}, {fieldsString}";
        }
    }
}
