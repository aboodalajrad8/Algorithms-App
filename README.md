# StudentAlgoApp

## ?? Description
A console application developed in C# (.NET 8) for the Algorithms practical exam.  
The project demonstrates the use of doubly linked lists and binary search trees (BST) to manage student records.

## ?? Features
- Add students at the beginning or end of the list.
- Sort students:
  - By Name (A ? Z).
  - By Average (lowest ? highest).
- Search students with an exact exam score (iterative, non-recursive).
- Filter students with average > 85.
- Delete a student by Id.
- Display all students from the list.
- Display students in-order by average using BST.

## ?? Data Structure Details
- Doubly Linked List: Supports insertion, deletion, sorting, searching, and filtering.
- Binary Search Tree (BST): Stores students by average, supports in-order traversal, filtering, and deletion.

## ?? Student Data
Each student record contains:
- Id (integer)
- Name (string)
- Governorate (string)
- First exam score (double)
- Second exam score (double)
- Average = (Exam1 + Exam2) / 2
- Grade (enum): Weak, Failing, Good, VeryGood, Excellent

## ?? How to Run
1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download).
2. Clone the repository:
   `bash
   git clone https://github.com/aboodalajrad8/Algorithms-App
   cd StudentAlgoApp



## Contrib

- Abdullah Al-Ajrad 
- Baraa Aita 