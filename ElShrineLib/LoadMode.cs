using System.Reflection;

namespace ElShrine
{
    [Flags]
    public enum LoadMode
    {
        None = 0,
        //Accessibility
        Public = 1, Private = 2, Protected = 4, //Internal = 8
        //Member type
        Property = 16, Field = 32, Method = 64, //_ = 128
        Class = 256, Attribute = 512, Interface = 1024, //_ = 2048 
        //Status
        Instance = 4096, Static = 8192, 
        //Special
        //Abstract = 16384, Generic = 32768,

        //Combined
        NonPublic = Protected | Private,
        AllAccessible = Public | Protected | Private,


        AllInstiateble = Instance | Static,
        
        PropertyAndField = Property | Field,
    }
    public static class LoadModeExtension
    {
        public static LoadMode GetMode(this object obj)
        {
            var result = LoadMode.None;
            if (obj is Type type) result = _getTypeMode(type);
            else if (obj is MethodInfo mi) result = _getMethodMode(mi);
            else if (obj is FieldInfo fi) result = _getFieldMode(fi);
            else if (obj is PropertyInfo pi) result = _getPropertyMode(pi);


            return result;
            static LoadMode _getMethodMode(MethodInfo methodInfo)
            {
                LoadMode mode = LoadMode.None;
                if (methodInfo.IsPublic) mode |= LoadMode.Public;
                else if (methodInfo.IsPrivate) mode |= LoadMode.Private;
                else if (methodInfo.IsFamily) mode |= LoadMode.Protected;
                mode |= methodInfo.IsStatic ? LoadMode.Static : LoadMode.Instance;
                mode |= LoadMode.Method;
                return mode;
            }
            static LoadMode _getFieldMode(FieldInfo fieldInfo)
            {
                LoadMode mode = LoadMode.None;
                if (fieldInfo.IsPublic) mode |= LoadMode.Public;
                else if (fieldInfo.IsPrivate) mode |= LoadMode.Private;
                else if (fieldInfo.IsFamily) mode |= LoadMode.Protected;
                mode |= fieldInfo.IsStatic ? LoadMode.Static : LoadMode.Instance;
                mode |= LoadMode.Field;
                return mode;
            }
            static LoadMode _getPropertyMode(PropertyInfo propertyInfo)
            {
                LoadMode mode = LoadMode.None;
                bool hasPublic = __hasAccessor(propertyInfo, m => m.IsPublic);
                bool hasProtected = __hasAccessor(propertyInfo, m => m.IsFamily);
                bool hasPrivate = __hasAccessor(propertyInfo, m => m.IsPrivate);

                if (hasPublic) mode |= LoadMode.Public;
                if (hasProtected) mode |= LoadMode.Protected;
                if (hasPrivate) mode |= LoadMode.Private;
                bool isStatic = propertyInfo.GetAccessors(true).Any(m => m.IsStatic);
                mode |= isStatic ? LoadMode.Static : LoadMode.Instance;
                mode |= LoadMode.Property;
                return mode;
                static bool __hasAccessor(PropertyInfo property, Func<MethodInfo, bool> condition) => property.GetAccessors(true).Any(condition);
            }
            static LoadMode _getTypeMode(Type type)
            {
                LoadMode mode = LoadMode.None;
                if (type.IsNested)
                {
                    if (type.IsNestedPublic) mode |= LoadMode.Public;
                    else if (type.IsNestedPrivate) mode |= LoadMode.Private;
                    else if (type.IsNestedFamily) mode |= LoadMode.Protected;
                }
                else if (type.IsPublic) mode |= LoadMode.Public;
                if (type.IsInterface) mode |= LoadMode.Interface;
                else if (type.IsClass)
                {
                    if (typeof(Attribute).IsAssignableFrom(type)) mode |= LoadMode.Attribute; // 特性类
                    else mode |= LoadMode.Class;
                }
                if (type.IsStaticClass()) mode |= LoadMode.Static;
                else mode |= LoadMode.Instance;
                return mode;
            }
        }

        public static bool ModeMatched(this object obj, LoadMode mode)
        {
            var objMode = GetMode(obj);
            return (mode & objMode) == objMode;
        }

        /// <summary>
        /// Validate whether <paramref name="obj"/>'s instiatebility mode is subset of <paramref name="mode"/> or not.
        /// </summary>
        public static bool ModeInstiatebleMatched(this object obj, LoadMode mode)
        {
            var objStatusMode = GetMode(obj) & LoadMode.AllInstiateble;
            var requiredStatusMode = mode & LoadMode.AllInstiateble;
            return requiredStatusMode == 0 || (requiredStatusMode & objStatusMode) == objStatusMode;
        }
        /// <summary>
        /// Validate whether <paramref name="obj"/>'s accessibility mode is subset of <paramref name="mode"/> or not.
        /// </summary>
        public static bool ModeAccessibleMatched(this object obj, LoadMode mode)
        {
            var objAccessMode = GetMode(obj) & LoadMode.AllAccessible;
            var requiredAccessMode = mode & LoadMode.AllAccessible;
            return requiredAccessMode == 0 || (objAccessMode != 0 && (requiredAccessMode & objAccessMode) == objAccessMode);
        }

        //ins4096 sta8192 | insf4 staf8
        public static BindingFlags ToInstiatebleFlags(this LoadMode mode)
        {
            var state = mode & LoadMode.AllInstiateble;
            return ((state & LoadMode.Instance) != 0 ? BindingFlags.Instance : 0) |
                   ((state & LoadMode.Static) != 0 ? BindingFlags.Static : 0);
        }
        public static BindingFlags ToAccessibilityFlags(this LoadMode mode)
        {
            var access = mode & LoadMode.AllAccessible;
            return ((access & LoadMode.Public) != 0 ? BindingFlags.Public : 0) |
                   ((access & (LoadMode.Private | LoadMode.Protected)) != 0 ? BindingFlags.NonPublic : 0);
        }

        public static BindingFlags ToSearchFlags(this LoadMode mode)
            => mode.ToInstiatebleFlags() | mode.ToAccessibilityFlags();
    }
}
