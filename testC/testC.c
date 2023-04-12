#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef enum bool{ false = 0, true = 1 } bool;
typedef struct A {
	int a;
}A;
void A_DeConstruct(A* this) {
	free(this);
}
typedef enum Extend$A {
	Extend$A$A,
	Extend$A$C,
}Extend$A;
typedef struct Typed$A {
	A* ptr;
	Extend$A type;
}Typed$A;
void Typed$A_DeConstruct(Typed$A* this) {
	free(this);
}
A* A_Construct() {
	A* this = (A*)malloc(sizeof(A));
	this->a = 1;
	return this;
}
void A_Add(A* this) {
	this->a = this->a + 1;
}
typedef struct Array$char {
	char* ptr;
	unsigned long len;
}Array$char;
void Array$char_DeConstruct(Array$char* this) {
	free(this->ptr);
	free(this);
}
typedef struct B {
	Array$char* b;
}B;
void B_DeConstruct(B* this) {
	Array$char_DeConstruct(this->b);
	free(this);
}
typedef struct C {
	int a;
	const char* id;
	B* b;
}C;
void C_DeConstruct(C* this) {
	B_DeConstruct(this->b);
	free(this);
}
void C_Add(C* this) {
	this->a = this->a + 2;
}
void B_Print(B* this) {
	printf("Hello World {");
	if (this == NULL) {
		printf("null");
	}
	else {
		printf("%s", this->b->ptr);
	}
	printf("}\n");
}
void Add(Typed$A* a) {
	if (a->type == Extend$A$A) {
		A_Add(a->ptr);
	}
	else if (a->type == Extend$A$C) {
		C_Add(a->ptr);
	}
	if (a->type == Extend$A$C) {
		C* c = a->ptr;
		B* b = c->b;
		B_Print(b);
		printf("add is type C\n");
	}
	if (a->type != Extend$A$C) {
		printf("add is not type C\n");
	}
	Typed$A_DeConstruct(a);
}
Typed$A* A_to_Typed$A(A* this) {
	Typed$A* t = (Typed$A*)malloc(sizeof(Typed$A));
	t->ptr = this;
	t->type = Extend$A$A;
	return t;
}
void TakeOwner(A* a) {
	printf("%i", a->a);
	printf("\n");
	A_DeConstruct(a);
}
A* A_BaseConstruct2(A* this, int a) {
	this->a = a;
	return this;
}
Array$char* str_toString(const char* this) {
	Array$char* newthis = (Array$char*)malloc(sizeof(Array$char));
	newthis->len = strlen(this);
	newthis->ptr = (char*)malloc(newthis->len + 1);
	memcpy(newthis->ptr, this, newthis->len + 1);
	return newthis;
}
B* B_Construct() {
	B* this = (B*)malloc(sizeof(B));
	Array$char* Converter = str_toString("Allo");
	this->b = Converter;
	return this;
}
C* C_Construct2(int a, const char* id) {
	C* this = (C*)malloc(sizeof(C));
	A_BaseConstruct2(this, a);
	this->id = id;
	this->b = B_Construct();
	return this;
}
A* A_BaseConstruct(A* this) {
	this->a = 1;
	return this;
}
C* C_Construct(const char* id) {
	C* this = (C*)malloc(sizeof(C));
	A_BaseConstruct(this);
	this->id = id;
	this->b = B_Construct();
	return this;
}
C* createC(int* a, const char* id) {
	if (a != NULL) {
		return C_Construct2(*a, id);
	}
	return C_Construct(id);
}
Typed$A* C_to_Typed$A(C* this) {
	Typed$A* t = (Typed$A*)malloc(sizeof(Typed$A));
	t->ptr = this;
	t->type = Extend$A$C;
	return t;
}
void A_Add2(A* this, int b) {
	this->a = this->a + b;
}
B* createB() {
	B* b = B_Construct();
	return b;
}
bool Array$char_StartsWith(Array$char* this, Array$char* o) {
	return o->len > this->len ? false : memcmp(this->ptr, o->ptr, o->len) == 0;
}
int main() {
	A* aa = A_Construct();
	printf("%i", aa->a);
	printf("\n");
	Typed$A* Converter = A_to_Typed$A(aa);
	Add(Converter);
	TakeOwner(aa);
	C* a = createC(NULL, "ID");
	printf("%s", a->id);
	printf(":\n");
	printf("%i", a->a);
	printf("\n");
	Typed$A* Converter2 = C_to_Typed$A(a);
	Add(Converter2);
	printf("%i", a->a);
	printf("\n");
	C_DeConstruct(a);
	int Converter3 = 5;
	a = createC(&Converter3, "ID2");
	C_DeConstruct(a);
	int Converter4 = 6;
	a = createC(&Converter4, "ID2,1");
	printf("%s", a->id);
	printf(":\n");
	printf("%i", a->a);
	printf("\n");
	A_Add2(a, 5);
	printf("%i", a->a);
	printf("\n");
	B* b = NULL;
	B_Print(b);
	if (b != NULL) {
		B_DeConstruct(b);
	}
	b = createB();
	B_Print(b);
	Array$char* Converter5 = str_toString(a->id);
	Array$char* start1 = Converter5;
	Array$char* Converter6 = str_toString("ID2");
	Array$char* start2 = Converter6;
	bool start = Array$char_StartsWith(start1, start2);
	printf("%i", start);
	printf("\n");
	C_DeConstruct(a);
	if (b != NULL) {
		B_DeConstruct(b);
	}
	Array$char_DeConstruct(start1);
	Array$char_DeConstruct(start2);
	return 0;
}
