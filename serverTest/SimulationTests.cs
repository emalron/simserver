using httpserver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace serverTest
{
    [TestClass]
    public class SimulationTests
    {
        [TestMethod]
        public void CountColor_WhenNoColor_ShouldThrowNullException()
        {
            // Arrange
            int width = 4, height = 1;
            Lattice[,] lattices = new Lattice[width, height];
            for(int i=0; i<width; i++)
            {
                lattices[i, 0] = new Lattice(i, 0, Color.Black);
            }
            Simulation simulation = new Simulation();
            Color searchColor = Color.White;

            // Act and Assert
            try
            {
                simulation.CountColor(lattices, searchColor);
            } catch(ArgumentNullException e)
            {
                StringAssert.Contains(e.Message, "no such color");
                return;
            }
            Assert.Fail("The expected expection was not thrown");
        }

        [TestMethod]
        public void CountColor_WhenFoundColor_ShouldReturnCount()
        {
            // Arrange
            int width = 4, height = 1;
            Lattice[,] lattices = new Lattice[width, height];
            for (int i = 0; i < width; i++)
            {
                lattices[i, 0] = new Lattice(i, 0, Color.Black);
            }
            lattices[3, 0].color = Color.White;
            Simulation simulation = new Simulation();
            Color searchColor = Color.White;
            int expected = 1;

            // Act and Assert
            int actual = simulation.CountColor(lattices, searchColor);
            Assert.AreEqual(expected, actual, 0.001, "matched");
        }

        [TestMethod]
        public void CountArray_CheckReturn()
        {
            // Arrange
            int width = 4, height = 4;
            Lattice[,] lattice = new Lattice[width, height];
            for (int j = 0; j < height; j++)
            {
                for (int i=0; i<width; i++)
                {
                    lattice[i, j] = new Lattice(i, j, Color.Black);
                }
            }
            Simulation simulation = new Simulation();
            int expected = width * height;

            // Act
            int actual = simulation.CountArray(lattice);

            // Assert
            Assert.AreEqual(expected, actual, 0, "return different value");
        }

        [TestMethod]
        public void CountArray_WhenHasMatchedColor_ShouldReturnNumber()
        {
            // Arrange
            int width = 4, height = 4;
            Lattice[,] lattice = new Lattice[width, height];
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    lattice[i, j] = new Lattice(i, j, Color.Black);
                }
            }
            Color target = Color.White;
            int expected = 0;
            for (int i=1; i<width-1; i++)
            {
                lattice[i, 0].color = target;
                expected++;
            }
            Simulation simulation = new Simulation();

            // Act
            int actual = simulation.CountColor(lattice, target);

            // Assert
            Assert.AreEqual(expected, actual, 0.001, "return different value");
        }
        [TestMethod]
        public void MatchedArray_CheckBoundary()
        {
            // Arrange
            int width = 4, height = 4;
            Lattice[,] lattice = new Lattice[width, height];
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    lattice[i, j] = new Lattice(i, j, Color.Black);
                }
            }
            Simulation simulation = new Simulation();

            // Act
            bool actual = simulation.MatchedArray(lattice);

            // Assert
            Assert.IsTrue(actual);
        }
    }
}
