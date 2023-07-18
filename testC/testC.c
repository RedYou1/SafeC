#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct A {
	int a;
}A;
A A_A_0() {
	A this;
	this.a = 1;
	return this;
}
typedef enum Extend$A {
	Extend$A$A,
	Extend$A$C,
}Extend$A;
typedef struct Typed$A {
	A* ptr;
	Extend$A type;
}Typed$A;
void A_Add(A* this) {
	this->a = this->a + 1;
}
typedef struct B {
	char* b;
}B;
typedef struct C {
	int a;
	char* id;
	B b;
}C;
void B_Print(B* this) {
	printf("Hello World {");
	if (this == 0) {
		printf("null");
	}
	else {
		printf(this->b);
	}
	printf("}\n");
}
void Add(Typed$A a) {
	A_Add(a.ptr);
	if (a.type == Extend$A$C) {
		C* c = a.ptr;
		B b = c->b;
		B_Print(&b);
		printf("add is type C\n");
	}
	else {
		printf("add is not type C\n");
	}
}
Typed$A A_to_Typed$A(A* t) {
	Typed$A this;
	this.ptr = t;
	this.type = Extend$A$A;
	return this;
}
void TakeOwner(A a) {
	printf("%i", a.a);
	printf("\n");
}
void A_Base_A_1(A* this, int a) {
	this->a = a;
}
B B_B_0() {
	B this;
	this.b = "Allo";
	return this;
}
C C_C_1(int a, char* id) {
	C this;
	A_Base_A_1(&this, a);
	this.id = id;
	this.b = B_B_0();
	return this;
}
void A_Base_A_0(A* this) {
	this->a = 1;
}
C C_C_0(char* id) {
	C this;
	A_Base_A_0(&this);
	this.id = id;
	this.b = B_B_0();
	return this;
}
C createC(int* a, char* id) {
	if (a != 0) {
		return C_C_1(*a, id);
	}
	return C_C_0(id);
}
Typed$A C_to_Typed$A(C* t) {
	Typed$A this;
	this.ptr = t;
	this.type = Extend$A$C;
	return this;
}
void A_Add2(A* this, int b) {
	this->a = this->a + b;
}
B createB() {
	B b = B_B_0();
	return b;
}
char StartsWith(char** this, char** s) {
}
int main() {
	A aa = A_A_0();
	printf("%i", aa.a);
	printf("\n");
	Add(A_to_Typed$A(&aa));
	TakeOwner(aa);
	C a = createC(0, "ID");
	printf(a.id);
	printf(":\n");
	printf("%i", a.a);
	printf("\n");
	Add(C_to_Typed$A(&a));
	printf("%i", a.a);
	printf("\n");
	int _0 = 5;
	a = createC(&_0, "ID2");
	int _1 = 6;
	a = createC(&_1, "ID2,1");
	printf(a.id);
	printf(":\n");
	printf("%i", a.a);
	printf("\n");
	A_Add2(&a, 5);
	printf("%i", a.a);
	printf("\n");
	B* b = 0;
	B_Print(b);
	B _2 = createB();
	b = &_2;
	B_Print(b);
	char* _3 = a.id;
	char* _4 = "ID2";
	char start = StartsWith(&_3, &_4);
	printf(start ? "True" : "False");
	printf("\n");
	return 0;
}
