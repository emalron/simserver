using System.Drawing;

namespace httpserver
{
    public class Lattice
    {
        public int x;
        public int y;
        public Color color;

        public Lattice(int x, int y, Color color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }
    }
}
