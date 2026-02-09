# 🚀 TINY Language Compiler

A comprehensive compiler front-end implementation designed for the **TINY programming language**. This project facilitates the translation of TINY source code into a structured hierarchical format through a complete lexical and syntax analysis pipeline.

## 📋 Project Overview
The compiler is built using a modular architecture that separates the responsibilities of scanning (tokenization) and parsing (syntax validation). It is designed to process the TINY language specification, which includes support for various data types, control structures, and function definitions.

## 🛠️ Key Components

### 🔍 Lexical Analysis (Scanner)
The Scanner is responsible for reading the raw input text and converting it into a sequence of meaningful tokens.
* **Reserved Keywords**: Detects language-specific keywords such as those used for declarations (int, float, string), I/O operations (read, write), and control flow (if, then, elseif, else, repeat, until, main).
* **Identifier Detection**: Identifies user-defined names for variables and functions, ensuring they follow the rule of starting with a letter.
* **Literal Recognition**: Processes numeric constants (both integers and floating-point values) and string literals.
* **Comment Handling**: Automatically identifies and filters out multi-line comments delimited by specific starting and ending symbols.
* **Symbol Recognition**: Tokenizes arithmetic operators, relational comparison symbols, logical operators, and the assignment symbol.

### 🏗️ Syntax Analysis (Parser)
The Parser receives the token stream and validates it against the formal grammar of the TINY language using a Recursive Descent approach.
* **Grammar Validation**: Ensures that the sequence of tokens follows the correct structural rules of the language.
* **Program Structure**: Enforces the requirement that a program consists of zero or more function definitions followed by a mandatory main entry point.
* **Statement Parsing**: Handles a variety of statement types, including variable declarations with optional initialization, assignment operations, and return statements.
* **Control Flow Logic**: Validates the structure of nested conditional blocks and loop constructs.
* **Expression Evaluation**: Manages complex mathematical and logical expressions, accounting for operator precedence and grouped operations.

### 🖥️ Visualization and UI
* **Parse Tree Generation**: As the parser validates the code, it builds a tree structure representing the syntactic derivation of the program.
* **Graphical Interface**: The project includes a visual tool to display the identified tokens and the resulting parse tree in a user-friendly window.

## 📝 TINY Language Features
The compiler is specifically tuned for the following language characteristics:

* **Data Types**: Full support for integer, floating-point, and string data.
* **Main Requirement**: Every valid program must conclude with a specific main function block.
* **Return Statements**: Functions are required to provide a return value at the end of their execution block.
* **Operator Set**: Includes a full suite of mathematical operations, logical comparisons (And/Or), and equality checks.

## 📂 Project Structure
* **Scanner Component**: Contains the logic for character-by-character scanning and token classification.
* **Parser Component**: Contains the recursive descent logic and the data structures for the tree nodes.
* **Compiler Driver**: Coordinates the scanning and parsing phases to execute the full compilation process.
* **Application Entry**: Manages the graphical user interface and the initial execution of the program.

## 🚀 Getting Started
To use the compiler, provide a source file written in the TINY language to the application. The system will then perform a full analysis, reporting any lexical or syntax errors found. If the code is valid, the system will generate and display a visual representation of the code's structure through the parse tree viewer.

## ⚠️ Error Reporting
The system provides detailed feedback for errors:
* **Lexical Errors**: Notifies the user of unrecognized symbols, unclosed comments, or invalid character sequences.
* **Syntax Errors**: Identifies structural issues such as missing punctuation, mismatched parentheses, or incorrect keyword usage.
