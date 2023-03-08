#include <stdio.h>
#include <stdlib.h>
#include <string.h>
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
const char* string_toStr(Array$char* this) {
	return this->ptr;
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
A* A_Construct2(int a) {
	A* this = (A*)malloc(sizeof(A));
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
	Array$char* Assign_Converter = str_toString("Allo");
	this->b = Assign_Converter;
	return this;
}
void B_DeConstruct(B* this) {
	Array$char_DeConstruct(this->b);
	free(this);
}
void B_Print(B* this) {
	printf("Hello World {");
	printf("%s", string_toStr(this->b));
	printf("}\n");
}
typedef struct C {
	int a;
	const char* id;
}C;
C* C_Construct(const char* id) {
	C* this = (C*)malloc(sizeof(C));
	{
		A* temp = A_Construct();
		memcpy(this, temp, sizeof(A));
		free(temp);
	}
	this->id = id;
	return this;
}
C* C_Construct2(int a, const char* id) {
	C* this = (C*)malloc(sizeof(C));
	{
		A* temp = A_Construct2(a);
		memcpy(this, temp, sizeof(A));
		free(temp);
	}
	this->id = id;
	return this;
}
void C_DeConstruct(C* this) {
	free(this);
}
int main() {
	C* a = C_Construct("ID");
	printf("%s", a->id);
	printf(":\n");
	printf("%i", a->a);
	printf("\n");
	A_Add(a);
	printf("%i", a->a);
	printf("\n");
	C* c = C_Construct2(5, "ID2");
	printf("%s", c->id);
	printf(":\n");
	printf("%i", c->a);
	printf("\n");
	A_Add2(c, 5);
	printf("%i", c->a);
	printf("\n");
	B* b = B_Construct();
	B_Print(b);
	C_DeConstruct(a);
	C_DeConstruct(c);
	B_DeConstruct(b);
	return 0;
}
