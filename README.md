# Math Structures
Representations of finite groups and n-dimensional vector fields,

## Finite Groups

A finite group consists of a finite list of elements of type `T`, and an operation of type `(T, T) -> T`. The `Group` class contains several types of utilities:

1. Determining whether the underlying list and operation constitute a valid group by confirming that they fulfill the group axioms: closure, associativity, identity, and inverse.
2. Computing generators and generator sets.
3. Computing subgroups, including cyclic and normal subgroups.
4. Computing quotients and left and right cosets.
5. Determining whether the group is abelian.
6. Computing the center, identity element, and inverse elements.

## Vector Fields

An n-dimensional vector field consists of a function that takes `n` numbers and outputs a vector of length `n`. The `VectorField` class models the function as a list of length `n` of functions that take `n` numbers and output a single number.
The `VectorField` class contains several types of utilities:

1. Computing the gradient (constructing a new `VectorField` using partial derivatives)
2. Computing the divergence (constructing a scalar function using partial derivatives)
3. Computing the curl for 3-dimensional fields (constructing a new `VectorField` using partial derivatives)

## Set Utilities

The `Group` class makes use of several utilities implementing set operations. These utilities are primarily implemented using `Linq`.

1. Operations on a single set: intersection, union, complement, difference, Cartesian product, and power set
2. Operations on two sets producing a function from set 1 to set 2: bijection, injection, surjection, and random map

## Derivative Utilities

The `VectorField` class uses a set of utilities to calculate the nth derivative of a function with respect to any given variable. These utilities model functions as `Expression` trees and traverse the body of a function, applying the appropriate differentiation rule (e.g. chain rule, product rule, etc.) at each node of the expression tree.

