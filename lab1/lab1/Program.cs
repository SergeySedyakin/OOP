using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    internal class Program
    {
        public class Point2d
        {
            private int x, y;
            
            public int X
            {
                get => x; 
                set
                {
                    if (0 > value || 100 < value)
                        throw new ArgumentOutOfRangeException("Недопустимый параметр координаты x");
                    else
                        x = value;
                }
            }
            public int Y
            {
                get => y;
                set
                {
                    if (0 > value || 1000 < value)
                        throw new ArgumentOutOfRangeException("Недопустимый параметр координаты y");
                    else
                        y = value;
                }
            }

            public Point2d(int X, int Y)
            {
                x = X;
                y = Y;
            }

            public static bool operator ==(Point2d p1, Point2d p2)
            {
                return p1.Equals(p2);
            }

            public static bool operator !=(Point2d p1, Point2d p2)
            {
                return !(p1 == p2);
            }

            public override string ToString()
            {
                return $"({x},{y})";
            }
        }

        public class Vector2d
        {
            public int X, Y;
            public Vector2d(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Vector2d(Point2d start, Point2d end)
            {
                X = end.X - start.X;
                Y = end.Y - start.Y;
            }

            // Индексатор
            public int this[int index]
            {
                get 
                {
                    switch (index)
                    {
                        case 0: return X;
                        case 1: return Y;
                        default: throw new IndexOutOfRangeException("Индекс должен быть 0 или 1");
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 0: 
                            X = value; 
                            break;
                        case 1: 
                            Y = value; 
                            break;
                        default: throw new IndexOutOfRangeException();
                    }
                }
            }

            // Итерирование
            public IEnumerator<int> GetEnumerator()
            {
                yield return X;
                yield return Y;
            }
            
            public static bool operator ==(Vector2d a, Vector2d b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Vector2d a, Vector2d b)
            {
                return !(a == b);
            }
            public override string ToString()
            {
                return $"({X}, {Y})";
            }
            public double ModulOfVector() => Math.Sqrt(X * X + Y * Y);

            public static Vector2d operator +(Vector2d a, Vector2d b) =>
                new Vector2d(a.X + b.X, a.Y + b.Y);

            public static Vector2d operator -(Vector2d a, Vector2d b) =>
                new Vector2d(a.X - b.X, a.Y - b.Y);

            public static Vector2d operator *(Vector2d v, int scalar) =>
                new Vector2d(v.X * scalar, v.Y * scalar);

            public static Vector2d operator *(int scalar, Vector2d v) => v * scalar;

            public static Vector2d operator /(Vector2d v, int scalar) => 
                new Vector2d(v.X / scalar, v.Y / scalar);

            public int ScalyrMulti(Vector2d v)
            {
                return X*v.X + Y*v.Y;
            }
            public static int ScalyrMulti(Vector2d a, Vector2d b)
            {
                return a.ScalyrMulti(b);
            }
            public int VectorMulti(Vector2d v)
            {
                return X*v.Y - Y*v.X;
            }
            public static int VectorMulti(Vector2d a, Vector2d b)
            {
                return a.VectorMulti(b);
            }

            public static int TripleProduct(Vector2d a, Vector2d b, Vector2d c)
            {
                return 0;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("ТОЧКИ");
            Point2d p1 = new Point2d(1,1);
            Console.WriteLine("первая точка: " + p1.ToString());
            Point2d p2 = new Point2d(3,4);
            Console.WriteLine("вторая точка: " + p2.ToString());

            Console.WriteLine("изменение точек");
            
            p1.X = 2;
            p1.Y = 2;
            Console.WriteLine("первая точка: " + p1.ToString());
            p2 = new Point2d(5, 6);
            Console.WriteLine("вторая точка: " + p2.ToString());

            Console.WriteLine("Проверка эквивалентности");
            
            Console.WriteLine("первая точка: " + p1.ToString());
            Console.WriteLine("вторая точка: " + p2.ToString());

            if (p1 == p2)
            {
                Console.WriteLine("эквиваленты");
            }
            else if(p1 != p2)
            {
                Console.WriteLine("не эквиваленты");
            }

            Console.WriteLine("ВЕКТОРЫ");
            Vector2d v1 = new Vector2d(4,4);
            Vector2d v2 = new Vector2d(p1,p2);
            Console.WriteLine("первый вектор " + v1.ToString());
            Console.WriteLine("второй вектор " + v2.ToString());

            Vector2d v = v1 + v2;
            Console.WriteLine("сумма векторов " + v.ToString());
            v = v2 - v1;
            Console.WriteLine("разность векторов " + v.ToString());
            Console.WriteLine("модуль первого вектора " + v1.ModulOfVector().ToString());
            v = v2 * 5;
            Console.WriteLine("Умножение второго вектора на 5: " + v.ToString());
            v = v1 / 2;
            Console.WriteLine("Деление на 2 первого вектора " + v.ToString());
            int scalyr = v1.ScalyrMulti(v2);
            int vectMulti = v1.VectorMulti(v2);
            Console.WriteLine("скалярное "+scalyr);
            Console.WriteLine("векторное "+vectMulti);

            Console.ReadKey();
        }
    }
}
