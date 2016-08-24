using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace XLinq
{
    internal class Program
    {
        private static XElement _element;

        private static void Main()
        {
            try
            {
                _element = new XElement("mscorlibContent");
                //2
                CreateAndDisplayXmlFormatForClasses();
                //3.a
                DisplayTypesWithoutProps();
                //3.b
                var sumMethods = CountTotalNumOf("Methods");
                Console.WriteLine($"Total Number of methods is: {sumMethods}");
                //3.c
                var sumProperties = CountTotalNumOf("Properties");
                Console.WriteLine($"Total Number of Properties is: {sumProperties}");
                FindAndDisplayTheMostCommonParam();
                //3.d
                var sortedTypes = GetTypesSortedByNumMethods();
                var newXml = new XElement("Types", sortedTypes);
                Console.WriteLine(newXml);
                //3.e
                GroupByNumMethodsAndDisplay();
                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void GroupByNumMethodsAndDisplay()
        {
            var typesWithNumMethods = from type in _element.Elements()
                let numOfMethods = type.Element("Methods").Elements().Count()
                select new {Type = type.FirstAttribute.Value, numMethods = numOfMethods};

            var groups = from @group in (from dType in typesWithNumMethods
                orderby dType.Type ascending
                group dType by dType.numMethods
                into newGroup
                select newGroup)
                orderby @group.Key descending
                select @group;

            foreach (var grouping in groups)
            {
                Console.WriteLine("**************");
                Console.WriteLine(grouping.Key);
                foreach (var el in grouping)
                {
                    Console.WriteLine(el);
                }
            }
        }

        private static IEnumerable<object> GetTypesSortedByNumMethods()
        {
            return from type in (from type in _element.Elements()
                let numOfProps = type.Element("Properties")?.Elements().Count()
                let numOfMethods = type.Element("Methods").Elements().Count()
                select new {Type = type.FirstAttribute.Value, NumOfPropreties = numOfProps, NumOfMethods = numOfMethods})
                orderby type.NumOfMethods descending
                select
                    new XElement("Type", new XAttribute("Name", type.Type),
                        new XAttribute("MethodsNumber", type.NumOfMethods),
                        new XAttribute("PropertiesNumber", type.NumOfPropreties));
        }

        private static void FindAndDisplayTheMostCommonParam()
        {
            var allMethods = from type in _element.Elements()
                from method in type.Element("Methods")?.Elements()
                select method;

            var allParams = from method in allMethods
                from parameter in method.Element("Parameters")?.Elements()
                select parameter;
            var groups = from @group in (from param in allParams
                group param by param.LastAttribute.Value
                into newGroup
                select newGroup)
                select new {Type = @group.Key, Count = @group.Count()};

            var maxCount = groups.Max(group => group.Count);
            var mostCommon = from @group in groups
                where @group.Count == maxCount
                select @group;
            Console.WriteLine(
                $"The most common parameter type: {mostCommon.First().Type},Count= {mostCommon.First().Count}");
        }

        private static int CountTotalNumOf(string ofWhat)
        {
            return (from elements in (from type in _element.Elements() select type.Element(ofWhat))
                select elements.Elements().Count()).Sum();
        }

        private static void DisplayTypesWithoutProps()
        {
            var typesWithoutProps = from xElement in _element.Elements()
                where !(xElement.Element("Properties")?.Elements()).Any()
                orderby xElement.Name
                select xElement;
            foreach (var el in typesWithoutProps)
            {
                Console.WriteLine(el);
            }
            Console.WriteLine($"Therer are {typesWithoutProps.Count()} types wihout properties");
        }

        private static void CreateAndDisplayXmlFormatForClasses()
        {
            var mscorlib = Assembly.Load("mscorlib ");
            var types = mscorlib.GetTypes().Where(type => !type.IsInterface && !type.IsValueType && type.IsPublic);
            foreach (var type in types)
            {
                try
                {
                    var newType = CreateXmlFromatOfType(type);
                    _element.Add(newType);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine(_element);
        }

        private static XElement CreateXmlFromatOfType(Type type)
        {
            var proprties =
                type.GetProperties().Select(prop => new XElement("Property", new XAttribute("Name", prop.Name)
                    , new XAttribute("Type", prop.GetType().Name)));
            var methods = type.GetMethods().Where(method => method.IsPublic)
                .Select(method => new XElement("Method", new XAttribute("Name", method.Name)
                    , new XAttribute("ReturnType", method.ReturnType.Name)
                    , new XElement("Parameters", method.GetParameters()
                        .Select(param => new XElement("Parameter", new XAttribute("Name", param.Name)
                            , new XAttribute("Type", param.ParameterType))))));
            var props = new XElement("Properties", proprties);
            var meths = new XElement("Methods", methods);
            var newType = new XElement("Type", new XAttribute("FullName", type.FullName), props, meths);
            return newType;
        }
    }
}
