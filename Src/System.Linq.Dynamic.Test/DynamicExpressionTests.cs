﻿using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Linq.Dynamic.Test
{
    [TestClass]
    public class DynamicExpressionTests
    {
        [TestMethod]
        public void Parse_ParameterExpressionMethodCall_ReturnsIntExpression()
        {
            var expression = DynamicExpression.Parse(
                new[] { Expression.Parameter(typeof(int), "x") },
                typeof(int),
                "x + 1");
            Assert.AreEqual(typeof(int), expression.Type);
        }

        [TestMethod]
        public void Parse_TupleToStringMethodCall_ReturnsStringLambdaExpression()
        {
            var expression = DynamicExpression.ParseLambda(
                typeof(Tuple<int>),
                typeof(string),
                "it.ToString()");
            Assert.AreEqual(typeof(string), expression.ReturnType);
        }

        [TestMethod]
        public void ParseLambda_DelegateTypeMethodCall_ReturnsEventHandlerLambdaExpression()
        {
            var expression = DynamicExpression.ParseLambda(
                typeof(EventHandler),
                new[] { Expression.Parameter(typeof(object), "sender"),
                        Expression.Parameter(typeof(EventArgs), "e") },
                null,
                "sender.ToString()");

            Assert.AreEqual(typeof(void), expression.ReturnType);
            Assert.AreEqual(typeof(EventHandler), expression.Type);
        }

        [TestMethod]
        public void ParseLambda_VoidMethodCall_ReturnsActionDelegate()
        {
            var expression = DynamicExpression.ParseLambda(
                typeof(System.IO.FileStream),
                null,
                "it.Close()");
            Assert.AreEqual(typeof(void), expression.ReturnType);
            Assert.AreEqual(typeof(Action<System.IO.FileStream>), expression.Type);
        }

        [TestMethod]
        public void CreateClass_TheadSafe()
        {
            const int numOfTasks = 15;

            var properties = new[] { new DynamicProperty("prop1", typeof(string)) };

            var tasks = new List<Task>(numOfTasks);

            for (var i = 0; i < numOfTasks; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => DynamicExpression.CreateClass(properties)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        [TestMethod]
        public void ShouldOrderSimpleTypes()
        {
            var ordered = Enumerable.Range(0, 100)
                .Select(i => new
                {
                    Value = $"{i:000}",
                })
                .OrderBy("Value desc");
            Assert.AreEqual(ordered.First().Value, "099");
            Assert.AreEqual(ordered.Last().Value, "000");
        }

        [TestMethod]
        public void ShouldOrderNestedTypes()
        {
            var ordered = new Outer[]
                {
                    new Outer(), 
                    new Outer
                    {
                        Value = "001 No inner",
                    },
                    new Outer
                    {
                        Value = "002 With inner",
                        Inner = new Inner(),
                    },
                    new Outer
                    {
                        Value = "003 With inner",
                        Inner = new Inner
                        {
                            Name = "With value",
                        },
                    },
                }
                .OrderBy("(Inner==null||Inner.Name==null?String(null):String(Inner.Name)) Asc,Value desc");
            Assert.AreEqual("002 With inner", ordered.First().Value);
            Assert.AreEqual("003 With inner", ordered.Last().Value);
        }

        private class Outer
        {
            public string Value { get; set; }
            public Inner Inner { get; set; }
        }

        private class Inner
        {
            public string Name { get; set; }
        }
    }
}
