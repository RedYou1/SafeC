#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef enum { false, true } bool;
typedef struct Array$char {
	char* ptr;
	unsigned long len;
}Array$char;
void Array$char_Construct(unsigned long len) {
	Array$char* this = (Array$char*)malloc(sizeof(Array$char));
	this->ptr = (char*)malloc(sizeof(char) * len);
	this->len = len;
	return this;
}
void Array$char_DeConstruct(Array$char* this) {
	free(this->ptr);
	free(this);
}
Array$char* str_toString(const char* this) {
	Array$char* newthis = (Array$char*)malloc(sizeof(Array$char));
	newthis->len = strlen(this);
	newthis->ptr = (char*)malloc(newthis->len + 1);
	memcpy(newthis->ptr, this, newthis->len + 1);
	return newthis;
}
typedef struct A {
	int a;
}A;
A* A_Construct() {
	A* this = (A*)malloc(sizeof(A));
	this->a = 1;
	return this;
}
A* A_BaseConstruct(A* this) {
	this->a = 1;
	return this;
}
A* A_Construct2(int a) {
	A* this = (A*)malloc(sizeof(A));
	this->a = a;
	return this;
}
A* A_BaseConstruct2(A* this, int a) {
	this->a = a;
	return this;
}
void A_DeConstruct(A* this) {
	free(this);
}
void A_Add(A* this) {
	this->a = this->a + 1;
}
void A_Add2(A* this, int b) {
	this->a = this->a + b;
}
typedef struct B {
	Array$char* b;
}B;
B* B_Construct() {
	B* this = (B*)malloc(sizeof(B));
	Array$char* Converter = str_toString("Allo");
	this->b = Converter;
	return this;
}
B* B_BaseConstruct(B* this) {
	Array$char* Converter = str_toString("Allo");
	this->b = Converter;
	return this;
}
void B_DeConstruct(B* this) {
	Array$char_DeConstruct(this->b);
	free(this);
}
void B_Print(B* this) {
	printf("Hello World {");
	printf("%s", this->b->ptr);
	printf("}\n");
}
typedef struct C {
	int a;
	const char* id;
	B* b;
}C;
C* C_Construct(const char* id) {
	C* this = (C*)malloc(sizeof(C));
	A_BaseConstruct(this);
	this->id = id;
	this->b = B_Construct();
	return this;
}
C* C_BaseConstruct(C* this, const char* id) {
	A_BaseConstruct(this);
	this->id = id;
	this->b = B_Construct();
	return this;
}
C* C_Construct2(int a, const char* id) {
	C* this = (C*)malloc(sizeof(C));
	A_BaseConstruct2(this, a);
	this->id = id;
	this->b = B_Construct();
	return this;
}
C* C_BaseConstruct2(C* this, int a, const char* id) {
	A_BaseConstruct2(this, a);
	this->id = id;
	this->b = B_Construct();
	return this;
}
void C_DeConstruct(C* this) {
	B_DeConstruct(this->b);
	free(this);
}
B* createB() {
	B* b = B_Construct();
	return b;
}
C* createC(int* a, const char* id) {
	if (a != NULL) {
		return C_Construct2(*a, id);
	}
	return C_Construct(id);
}
int main() {
	C* a = createC(NULL, "ID");
	printf("%s", a->id);
	printf(":\n");
	printf("%i", a->a);
	printf("\n");
	A_Add(a);
	printf("%i", a->a);
	printf("\n");
	C_DeConstruct(a);
	int Converter = 5;
	a = createC(&Converter, "ID2");
	C_DeConstruct(a);
	int Converter2 = 6;
	a = createC(&Converter2, "ID3");
	printf("%s", a->id);
	printf(":\n");
	printf("%i", a->a);
	printf("\n");
	A_Add2(a, 5);
	printf("%i", a->a);
	printf("\n");
	B* b = createB();
	B_Print(b);
	C_DeConstruct(a);
	B_DeConstruct(b);
	return 0;
}
