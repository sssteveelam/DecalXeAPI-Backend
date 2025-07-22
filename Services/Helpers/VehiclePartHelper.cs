// DecalXeAPI/Services/Helpers/VehiclePartHelper.cs
using System.ComponentModel;
using System.Reflection;
using DecalXeAPI.Models;

namespace DecalXeAPI.Services.Helpers
{
    public static class VehiclePartHelper
    {
        public static string GetDescription(VehiclePart vehiclePart)
        {
            FieldInfo? field = vehiclePart.GetType().GetField(vehiclePart.ToString());
            if (field != null)
            {
                DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }
            return vehiclePart.ToString();
        }

        public static Dictionary<VehiclePart, string> GetAllWithDescriptions()
        {
            var result = new Dictionary<VehiclePart, string>();
            foreach (VehiclePart part in Enum.GetValues<VehiclePart>())
            {
                result[part] = GetDescription(part);
            }
            return result;
        }
    }
}
