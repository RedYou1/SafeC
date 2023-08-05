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
	Classes$A,
	Classes$B,
	Classes$C,
	Classes$Typed$A,
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
	A_Add2(&a, 5);
	return 0;
}
