using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentAlgoApp
{
    // Grade enum to store the qualitative evaluation for the final average
    // Note: The thresholds match typical ranges and the requirement to treat >=85 as "Excellent".
    public enum Grade
    {
        Weak,      // < 50
        Failing,   // 50–59
        Good,      // 60–74
        VeryGood,  // 75–84
        Excellent  // >= 85
    }

    /// <summary>
    /// Student model.
    /// Keeps the raw fields and computes the average + grade on demand.
    /// </summary>
    public class Student
    {
        // Keeping int for Id as the spec implies numeric student number.
        // If needed, can switch to Guid for strong uniqueness.
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;

        public double FirstExam { get; set; }
        public double SecondExam { get; set; }

        // Average is (FirstExam + SecondExam) / 2, rounded to 2 decimals for neat display
        public double Average => Math.Round((FirstExam + SecondExam) / 2.0, 2);

        // Grade is derived from Average, no manual input needed (can be swapped to manual enum if requested)
        public Grade Grade
        {
            get
            {
                var avg = Average;
                if (avg >= 85) return Grade.Excellent;
                if (avg >= 75) return Grade.VeryGood;
                if (avg >= 60) return Grade.Good;
                if (avg >= 50) return Grade.Failing;
                return Grade.Weak;
            }
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Governorate: {Governorate}, Exam1: {FirstExam}, Exam2: {SecondExam}, Average: {Average}, Grade: {Grade}";
        }
    }

    /// <summary>
    /// Doubly linked list node that wraps a Student record.
    /// </summary>
    public class DoublyNode
    {
        public Student Data;
        public DoublyNode Prev;
        public DoublyNode Next;

        public DoublyNode(Student s) { Data = s; }
    }

    /// <summary>
    /// Doubly linked list for student management.
    /// Supports: add at beginning/end, remove by Id, enumerate, sort by name/average, search by exact exam score, filter > threshold.
    /// </summary>
    public class DoublyLinkedList
    {
        public DoublyNode Head { get; private set; }
        public DoublyNode Tail { get; private set; }
        public int Count { get; private set; }

        /// <summary>
        /// Add a student at the very beginning (head) of the list.
        /// </summary>
        public void AddFirst(Student s)
        {
            var node = new DoublyNode(s);

            if (Head == null)
            {
                Head = Tail = node;
            }
            else
            {
                node.Next = Head;
                Head.Prev = node;
                Head = node;
            }
            Count++;
        }

        /// <summary>
        /// Add a student at the very end (tail) of the list.
        /// </summary>
        public void AddLast(Student s)
        {
            var node = new DoublyNode(s);

            if (Tail == null)
            {
                Head = Tail = node;
            }
            else
            {
                node.Prev = Tail;
                Tail.Next = node;
                Tail = node;
            }
            Count++;
        }

        /// <summary>
        /// Remove a student by student Id. Returns true if removed, false if not found.
        /// </summary>
        public bool RemoveById(int id)
        {
            var curr = Head;

            while (curr != null)
            {
                if (curr.Data.Id == id)
                {
                    var prev = curr.Prev;
                    var next = curr.Next;// Link neighbors around the current node
                    if (prev != null) prev.Next = next; else Head = next;
                    if (next != null) next.Prev = prev; else Tail = prev;

                    Count--;
                    return true;
                }
                curr = curr.Next;
            }
            return false;
        }

        /// <summary>
        /// Enumerate all students from head to tail.
        /// </summary>
        public IEnumerable<Student> Enumerate()
        {
            var curr = Head;
            while (curr != null)
            {
                yield return curr.Data;
                curr = curr.Next;
            }
        }

        /// <summary>
        /// Sort the list by Name ascending (A→Z).
        /// Implementation: materialize into a List, sort, then rebuild the linked list.
        /// </summary>
        public void SortByNameAsc()
        {
            var list = Enumerate().ToList();
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            RebuildFromList(list);
        }

        /// <summary>
        /// Sort the list by Average ascending (lowest → highest).
        /// Implementation: materialize into a List, sort, then rebuild the linked list.
        /// </summary>
        public void SortByAverageAsc()
        {
            var list = Enumerate().ToList();
            list.Sort((a, b) => a.Average.CompareTo(b.Average));
            RebuildFromList(list);
        }

        /// <summary>
        /// Search (iterative, non-recursive) for students who have an exact exam score
        /// either in the first or second exam.
        /// </summary>
        public List<Student> SearchByExactExamScore(double score)
        {
            var result = new List<Student>();
            var curr = Head;

            while (curr != null)
            {
                // Using tolerance for floating point comparison
                if (Math.Abs(curr.Data.FirstExam - score) < 1e-9 || Math.Abs(curr.Data.SecondExam - score) < 1e-9)
                    result.Add(curr.Data);

                curr = curr.Next;
            }
            return result;
        }

        /// <summary>
        /// Filter students with Average greater than a given threshold.
        /// </summary>
        public List<Student> FilterAverageAbove(double threshold)
        {
            var res = new List<Student>();
            var curr = Head;

            while (curr != null)
            {
                if (curr.Data.Average > threshold) res.Add(curr.Data);
                curr = curr.Next;
            }
            return res;
        }

        /// <summary>
        /// Helper: rebuild the linked list from a sorted list to reflect the new order.
        /// </summary>
        private void RebuildFromList(List<Student> list)
        {
            Head = Tail = null;
            Count = 0;

            foreach (var s in list)
                AddLast(s);
        }
    }

    /// <summary>
    /// BST node keyed by Average. For duplicate averages, store a list of students in the node.
    /// </summary>
    public class BstNode
    {
        public double KeyAverage;
        public List<Student> Students = new List<Student>();
        public BstNode Left;
        public BstNode Right;

        public BstNode(double avg, Student s)
        {
            KeyAverage = avg;
            Students.Add(s);
        }
    }

    /// <summary>
    /// Student BST indexed by Average: insert, remove by Id (scan nodes), in-order traversal, filter > threshold.
    /// </summary>
    public class StudentBst
    {
        public BstNode Root { get; private set; }

        /// <summary>
        /// Insert a student into the BST by Average.
        /// </summary>
        public void Insert(Student s)
        {
            Root = InsertInternal(Root, s);
        }

        private BstNode InsertInternal(BstNode node, Student s)
        {
            if (node == null) return new BstNode(s.Average, s); if (s.Average < node.KeyAverage) node.Left = InsertInternal(node.Left, s);
            else if (s.Average > node.KeyAverage) node.Right = InsertInternal(node.Right, s);
            else node.Students.Add(s); // same average → keep in this node

            return node;
        }

        /// <summary>
        /// Remove a student by Id anywhere in the tree.
        /// Note: We scan all nodes (safe and simple). For large trees, we can optimize later.
        /// </summary>
        public bool RemoveById(int id)
        {
            bool removed = false;

            void TraverseAndRemove(BstNode n)
            {
                if (n == null) return;

                n.Students.RemoveAll(st =>
                {
                    var match = st.Id == id;
                    if (match) removed = true;
                    return match;
                });

                TraverseAndRemove(n.Left);
                TraverseAndRemove(n.Right);
            }

            TraverseAndRemove(Root);
            return removed;
        }

        /// <summary>
        /// Return all students in ascending order by Average (in-order traversal).
        /// </summary>
        public IEnumerable<Student> InOrder()
        {
            foreach (var s in InOrderInternal(Root))
                yield return s;
        }

        private IEnumerable<Student> InOrderInternal(BstNode node)
        {
            if (node == null) yield break;

            foreach (var s in InOrderInternal(node.Left)) yield return s;
            foreach (var s in node.Students) yield return s;
            foreach (var s in InOrderInternal(node.Right)) yield return s;
        }

        /// <summary>
        /// Filter students with Average greater than a given threshold.
        /// </summary>
        public List<Student> FilterAverageAbove(double threshold)
        {
            var res = new List<Student>();

            void Walk(BstNode n)
            {
                if (n == null) return;
                if (n.KeyAverage > threshold) res.AddRange(n.Students);
                Walk(n.Left);
                Walk(n.Right);
            }

            Walk(Root);
            return res;
        }
    }

    /// <summary>
    /// Console UI. Provides a clear menu with friendly messages for normal users.
    /// </summary>
    internal class Program
    {
        // Data structures used in parallel as per the spec:
        // - Doubly linked list for core operations (insert at head/tail, sort, search, delete)
        // - BST to "redo" the problem using a tree, including in-order display and filtering >85
        private static readonly DoublyLinkedList list = new DoublyLinkedList();
        private static readonly StudentBst bst = new StudentBst();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            PrintHeader();
            Console.WriteLine("Please enter 5 students to initialize the data:");

            // Initial input: exactly 5 students, as requested
            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine($"\n[Student #{i}]");
                var s = ReadStudentFromUser();
                list.AddLast(s);
                bst.Insert(s);
                Console.WriteLine("Student added successfully.");
            }

            // Main loop: user selects operations
            while (true)
            {
                PrintMenu();

                Console.Write("Your choice: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        list.SortByNameAsc();
                        Console.WriteLine("Sorted by name A→Z.");
                        break;

                    case "2":
                        list.SortByAverageAsc();
                        Console.WriteLine("Sorted by average (lowest to highest).");
                        break;
                    case "3":
                        Console.Write("Enter the exact exam score to search for (e.g., 75): ");
                        if (double.TryParse(Console.ReadLine(), out var score))
                        {
                            var found = list.SearchByExactExamScore(score);
                            if (found.Count == 0) Console.WriteLine("No students found with that exact score.");
                            else PrintStudents(found, "Search results:");
                        }
                        else Console.WriteLine("Invalid number.");
                        break;

                    case "4":
                        {
                            var s = ReadStudentFromUser();
                            list.AddFirst(s);
                            bst.Insert(s);
                            Console.WriteLine("Student added at the beginning of the list.");
                        }
                        break;

                    case "5":
                        {
                            var s = ReadStudentFromUser();
                            list.AddLast(s);
                            bst.Insert(s);
                            Console.WriteLine("Student added at the end of the list.");
                        }
                        break;

                    case "6":
                        {
                            var high = list.FilterAverageAbove(85);
                            if (high.Count == 0) Console.WriteLine("No students with average > 85.");
                            else PrintStudents(high, "Students with average > 85:");
                        }
                        break;

                    case "7":
                        Console.Write("Enter the student Id to delete: ");
                        if (int.TryParse(Console.ReadLine(), out var id))
                        {
                            var ok1 = list.RemoveById(id);
                            var ok2 = bst.RemoveById(id);
                            Console.WriteLine(ok1 || ok2 ? "Student deleted." : "No student found with that Id.");
                        }
                        else Console.WriteLine("Invalid Id.");
                        break;

                    case "8":
                        {
                            var all = list.Enumerate().ToList();
                            if (all.Count == 0) Console.WriteLine("The list is empty.");
                            else PrintStudents(all, "All students (from list):");
                        }
                        break;

                    case "9":
                        {
                            var ordered = bst.InOrder().ToList();
                            if (ordered.Count == 0) Console.WriteLine("The tree is empty.");
                            else PrintStudents(ordered, "In-order (by average, from BST):");
                        }
                        break;

                    case "0":
                        Console.WriteLine("Thank you for using the system. Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Please choose a valid option (0–9).");
                        break;
                }
            }
        }

        // ===== Input helpers (user-friendly and safe) =====

        /// <summary>
        /// Reads a student from the console with validation and friendly prompts.
        /// </summary>
        private static Student ReadStudentFromUser()
        {
            var s = new Student
            {
                Id = ReadInt("Enter student Id (integer): "),
                Name = ReadNonEmpty("Enter student name: "),
                Governorate = ReadNonEmpty("Enter governorate: "),
                FirstExam = ReadDoubleInRange("Enter first exam score (0–100): ", 0, 100),
                SecondExam = ReadDoubleInRange("Enter second exam score (0–100): ", 0, 100)
            };// Display computed average and grade for immediate feedback
            Console.WriteLine($"Average = {s.Average}, Grade = {s.Grade}");

            // TODO: If needed, prompt the user to pick Grade enum manually (instead of computed).
            return s;
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var v)) return v;
                Console.WriteLine("The input is not a valid integer. Try again.");
            }
        }

        private static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var v = Console.ReadLine() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
                Console.WriteLine("This field cannot be empty.");
            }
        }

        private static double ReadDoubleInRange(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out var v) && v >= min && v <= max) return v;
                Console.WriteLine($"Please enter a value between {min} and {max}.");
            }
        }

        // ===== Output helpers =====

        private static void PrintHeader()
        {
            Console.WriteLine("Student Management — Algorithms (Doubly Linked List + BST)");
            Console.WriteLine("Features: Insert (head/tail), Delete by Id, Sort by Name/Average, Non-recursive Search, Filter >85, In-order from BST.");
            Console.WriteLine("----------------------------------------------------------------");
        }

        private static void PrintMenu()
        {
            Console.WriteLine("\nChoose an operation:");
            Console.WriteLine("1) Sort by name A→Z (List)");
            Console.WriteLine("2) Sort by average (lowest → highest) (List)");
            Console.WriteLine("3) Search students with an exact exam score (iterative, non-recursive)");
            Console.WriteLine("4) Add a new student at the beginning");
            Console.WriteLine("5) Add a new student at the end");
            Console.WriteLine("6) Show students with average > 85");
            Console.WriteLine("7) Delete a student by Id");
            Console.WriteLine("8) Show all students (from list)");
            Console.WriteLine("9) Show in-order by average using the tree (BST)");
            Console.WriteLine("0) Exit");
        }

        private static void PrintStudents(IEnumerable<Student> students, string title)
        {
            Console.WriteLine($"\n{title}");
            Console.WriteLine(new string('-', 60));

            foreach (var s in students)
                Console.WriteLine(s.ToString());

            Console.WriteLine(new string('-', 60));
        }
    }
}