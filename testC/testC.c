#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct INumber INumber;
typedef struct A {
	int a;
}A;
A A_A_0() {
	A this;
	this.a = 1;
	return this;
}
typedef enum Classes {
	Classes$INumber,
	Classes$Colors,
	Classes$Test,
	Classes$A,
	Classes$B,
	Classes$C,
	Classes$Lock,
	Classes$Typed$A,
	Classes$Container$int,
	Classes$Container$Colors,
}Classes;
typedef struct Typed$A {
	A* ptr;
	Classes type;
}Typed$A;
typedef enum Colors {
	Colors$blue,
	Colors$red,
	Colors$yellow,
	Colors$green,
}Colors;
typedef struct B {
	char* b;
	Colors color;
}B;
typedef struct C {
	int a;
	char* id;
	B b;
}C;
void A_Add(A* this) {
	this->a = this->a + 1;
}
void B_Print(B* this) {
	printf("Hello World {");
	if (this == 0) {
		printf("null");
	}
	else {
		printf(this->b);
		printf(", ");
		printf("%i", this->color);
	}
	printf("}\n");
}
void Add(Typed$A a) {
	A_Add(a.ptr);
	if (a.type == Classes$C) {
		C* c = a.ptr;
		B b = c->b;
		B_Print(&b);
		printf("add is type C\n");
	}
	else {
		printf("add is not type C\n");
	}
}
Typed$A Typed$A$A(A* ptr) {
	Typed$A this;
	this.ptr = ptr;
	this.type = Classes$A;
	return this;
}
void TakeOwner(A a) {
	printf("%i", a.a);
	printf("\n");
}
void A_Base_A_1(A* this, int a) {
	this->a = a;
}
B B_B_0(Colors color) {
	B this;
	this.b = "Allo";
	this.color = color;
	return this;
}
C C_C_1(int a, char* id) {
	C this;
	A_Base_A_1(&this, a);
	this.id = id;
	this.b = B_B_0(Colors$yellow);
	return this;
}
void A_Base_A_0(A* this) {
	this->a = 1;
}
C C_C_0(char* id) {
	C this;
	A_Base_A_0(&this);
	this.id = id;
	this.b = B_B_0(Colors$green);
	return this;
}
C createC(int* a, char* id) {
	if (a != 0) {
		return C_C_1(*a, id);
	}
	return C_C_0(id);
}
Typed$A Typed$A$C(C* ptr) {
	Typed$A this;
	this.ptr = ptr;
	this.type = Classes$C;
	return this;
}
void A_Add2(A* this, int b) {
	this->a = this->a + b;
}
B createB(Colors color) {
	B b = B_B_0(color);
	return b;
}
char StartsWith(char* this, char* s) {
	return strncmp(s, this, strlen(s)) == 0;
}
unsigned long Len(char* this) {
	return strlen(this);
}
typedef struct Lock {
	int l;
	Typed$A a;
}Lock;
Lock Lock_Lock_0(int l, Typed$A a) {
	Lock this;
	this.l = l;
	this.a = a;
	Add(this.a);
	return this;
}
typedef struct Container$int {
	int a;
}Container$int;
void Container$int_test$Colors(Container$int this, Colors b) {
	printf("Container{");
	printf("%i", this.a);
	printf(", ");
	printf("%i", b);
	printf("}");
}
Container$int Container$int_Container_0$Colors(int a, Colors* b) {
	Container$int this;
	this.a = a;
	Container$int_test$Colors(this, *b);
	return this;
}
typedef struct Container$Colors {
	Colors a;
}Container$Colors;
void Container$Colors_test$int(Container$Colors this, int b) {
	printf("Container{");
	printf("%i", this.a);
	printf(", ");
	printf("%i", b);
	printf("}");
}
Container$Colors Container$Colors_Container_0$int(Colors a, int* b) {
	Container$Colors this;
	this.a = a;
	Container$Colors_test$int(this, *b);
	return this;
}
typedef struct Test {
	unsigned char c;
	union {
		char a;
		int b;
	};
}Test;
Test Test_Test_0(char a) {
	Test this;
	this.c = 0;
	this.a = a;
	return this;
}
void Test_Print(Test this) {
	if (this.c == 0) {
		printf("%c", this.a);
		printf("%c", '\n');
	}
	else {
		printf("%i", this.b);
		printf("%c", '\n');
	}
}
Test Test_Test_1(int b) {
	Test this;
	this.c = 1;
	this.b = b;
	return this;
}
int main() {
	A aa = A_A_0();
	printf("%i", aa.a);
	printf("\n");
	Add(Typed$A$A(&aa));
	TakeOwner(aa);
	C a = createC(0, "ID");
	printf(a.id);
	printf(":\n");
	printf("%i", a.a);
	printf("\n");
	Add(Typed$A$C(&a));
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
	B _2 = createB(Colors$blue);
	b = &_2;
	B_Print(b);
	B _3 = createB(Colors$red);
	b = &_3;
	B_Print(b);
	char start = StartsWith(a.id, "ID2");
	printf(start ? "True" : "False");
	printf("\n");
	A_Add2(&a, 5);
	unsigned long sl = Len(a.id);
	printf("%lu", sl);
	printf("\n");
	Lock lock = Lock_Lock_0(5, Typed$A$C(&a));
	Colors _4 = Colors$blue;
	Container$int_Container_0$Colors(32, &_4);
	printf("\n");
	int _5 = 16;
	Container$Colors_Container_0$int(Colors$red, &_5);
	printf("%c", '\n');
	Test t = Test_Test_0('A');
	Test_Print(t);
	t = Test_Test_1(45);
	Test_Print(t);
	return 0;
}
