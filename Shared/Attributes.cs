using System;

namespace SeleniumDotNetEngine.Shared
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SeleniumTestAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class StartupAttribute : Attribute { }
}
