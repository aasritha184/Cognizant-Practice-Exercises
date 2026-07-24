using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello World");

        int score = 82;
        string grade = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : "F";
        Console.WriteLine("Grade: " + grade);

        int[] nums = { 1, 2, 3, 4, 5 };
        foreach (var n in nums) Console.Write(n + " ");
        Console.WriteLine();

        Console.WriteLine(Add(2, 3));

        int a = 5;
        AddTen(ref a);
        Console.WriteLine(a);

        var car = new Car("Toyota", "Corolla", 2024);
        Console.WriteLine(car.Make + " " + car.Model);

        var employee = new Employee("Sam", "Engineering");
        var moved = employee with { Department = "Marketing" };
        Console.WriteLine(moved.Department);

        Shape s = new Circle();
        s.Draw();

        var orders = new List<Order> { new(1, "Alice", 120), new(2, "Bob", 45) };
        var big = orders.Where(o => o.Total > 100);
        foreach (var o in big) Console.WriteLine(o.Customer);

        var user = new User { Name = "Sam", Age = 28 };
        string json = JsonSerializer.Serialize(user);
        Console.WriteLine(json);

        int counter = 0;
        var lockObj = new object();
        var threads = new List<Thread>();
        for (int i = 0; i < 3; i++)
        {
            var t = new Thread(() => { lock (lockObj) { counter++; } });
            threads.Add(t);
            t.Start();
        }
        foreach (var t in threads) t.Join();
        Console.WriteLine("Counter: " + counter);

        string input = "<script>alert('x')</script>";
        Console.WriteLine(System.Net.WebUtility.HtmlEncode(input));
    }

    static int Add(int a, int b) => a + b;
    static void AddTen(ref int x) => x += 10;

    class Car
    {
        public string Make, Model;
        public int Year;
        public Car(string make, string model, int year) { Make = make; Model = model; Year = year; }
    }

    record Employee(string Name, string Department);
    record Order(int Id, string Customer, decimal Total);

    abstract class Shape { public virtual void Draw() => Console.WriteLine("Shape"); }
    class Circle : Shape { public override void Draw() => Console.WriteLine("Circle"); }

    class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    static void AdoCrudExample()
    {
        string connStr = "Server=localhost;Database=EventPortal;Trusted_Connection=True;";
        using var conn = new SqlConnection(connStr);
        conn.Open();

        using var insert = new SqlCommand("INSERT INTO Employees (Name) VALUES (@name)", conn);
        insert.Parameters.AddWithValue("@name", "Jordan Lee");
        insert.ExecuteNonQuery();

        using var select = new SqlCommand("SELECT * FROM Employees", conn);
        using var reader = select.ExecuteReader();
        while (reader.Read()) Console.WriteLine(reader["Name"]);
    }
}
