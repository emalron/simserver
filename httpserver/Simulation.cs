using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace httpserver
{
    public class Simulation
    {
        public int CountColor(Lattice[,] lattice, Color target)
        {
            int width = lattice.GetLength(0);
            int height = lattice.GetLength(1);
            Lattice first = null;
            for (int j=0; j<height; j++)
            {
                for (int i=0; i<width; i++)
                {
                    if (lattice[i, j].color == target)
                    {
                        first = lattice[i, j];
                        break;
                    }
                }
            }
            if (first == null)
            {
                throw new ArgumentNullException("target", "no such color");
            }

            int count = 1;

            Queue<Lattice> q = new Queue<Lattice>();
            bool[,] visited = new bool[lattice.GetLength(0), lattice.GetLength(1)];
            int[] dx = { 1, 0, -1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            q.Enqueue(first);
            visited[first.x, first.y] = true;
            while(q.Count > 0)
            {
                Lattice now = q.Dequeue();
                for(int i=0; i<4; i++)
                {
                    int nx = now.x + dx[i], ny = now.y + dy[i];
                    bool isInside = nx >= 0 && nx < width && ny >= 0 && ny < height;
                    if(!isInside)
                    {
                        continue;
                    }
                    Lattice current = lattice[nx, ny];
                    bool isMatched = visited[nx, ny] == false && current.color == target;
                    if (isMatched)
                    {
                        q.Enqueue(current);
                        count++;
                    }
                    visited[nx, ny] = true;
                }
            }
            return count;
        }

        public int CountArray(Lattice[,] lattice)
        {
            return lattice.Length;
        }

        public bool MatchedArray(Lattice[,] lattice)
        {
            bool[,] visited = new bool[lattice.GetLength(0), lattice.Rank];
            try
            {
                for (int j = 0; j < lattice.Rank; j++)
                {
                    for (int i = 0; i < lattice.GetLength(j); i++)
                    {
                        visited[i, j] = true;
                    }
                }
            } catch(ArgumentOutOfRangeException e)
            {
                throw e;
            }
            return true;
        }
    }
}
