﻿using NUnit.Framework;

namespace DotJEM.Json.Index.Test
{
    public class MyFrameworkClassTest
    {
        [Test]
        public void SayHello_ReturnsHello()
        {
            Assert.That(new MyFrameworkClass().SayHello(), Is.EqualTo("Hello"));
        }
    }
}