# Norma Programming Language

> \*\***Find Article about making it [HashNode](https://jbrosdev.hashnode.dev/guide-to-building-your-own-programming-language-with-c)**\*\*

Norma is a programming language built in C# made to be extremely simple. It's syntax is similar to Python and JavaScript, but has my own custom spin on it. 

Here is the [Nuget Package](https://www.nuget.org/packages/Norma/) if you want to add it to your C# project
> `dotnet add package Norma`

<p align="center">
  <a>
    <img src="https://raw.githubusercontent.com/JBrosDevelopment/Norma/main/images/Norma%20(526px).png" alt="icon">
  </a>
</p>

Norma is a very simple language. To define a variable, use:
```js
var x = 10
```
To access the variable put it in between `$`
```js
var y = $x$
```
To create a pointer to that variable, use the `point` keyword. 
```js
var z = point: $x$
```
To call a function, don't use parenthesis.
```js
print "Hello World"
```
Strings have automatic interpolation with variables using the `$` syntax.
```js
print "X is equal to $x$ and y is equal to $y$"
```
To define a function, use `def`
```js
def add: left, right {
    return ($left$ + $right$)
}

add 5, 6
```
Any equations must be inside `( )`.
```js
var x = ($y$ * $z$)
```
To manipulate a variable, use `+` instead of `+=` where `+` can be any operator
```js
var x = 10
x + 5
x - 4
x * 3
x / 2
x = 1

print $x$
```
Comments are very simple
```js
// Line Comment
/*
    Block Comment
*/
```
Arrays are also allowed in this language
```js
var y = [0, 1, 2]
```
They are typeless and can hold anything including pointers
```js
y = [0, "yes", ["no", $z$], point: $x$]
```
Structs are the next main feature
```js
struct Vector { X, Y, Z }

var vec = Vector: 100, 5.23, 99
```
They can be accessed and modified easily,
```js
vec.X = 150
vec.Y = $vec.X$
```
The values are also typeless and can be anything
```js
vec.X = "Hello World"
vec.Y = point: $z$
vec.Z = [1, 2, 3]
```
Statements are straightforward as well
```py
if x > 55 { 
    print "X is big"
}
elif x < 0 {
    print "X is negative"
}
else { 
    print "x is small"
}
```
There are loops as well
```js
while true {
    print "yes"
}
for i in 150 {
    print $i$
}
```
For loops can also use arrays 
```js
for value in $array$ {
    print $value$
}
```
There are many built in functions as well ([Test file](FunctionTesting.norm)):
- `print "text"`
- `input`
- `clear`
- `exit`
- `substring "text", $start$, $len$`
- `indexOf "text", "e"`
- `toLower "Text"`
- `toUpper "text"`
- `trim " text "`
- `revere "string or array"`
- `length ["string", "or", "array"]`
- `append [0, 1], 5`
- `remove $index$`
- `insert $array$, $index$, $value$`
- `slice [99, 98, 97, 96, 95], $start$, $len$ `
- `concat $array$, [0, 1, 2, 3]`
- `sort [9, 5, 7, 2, 0]`
