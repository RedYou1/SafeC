#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct {
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
typedef struct {
	char* b;
}B;
B* B_Construct() {
	B* this = (B*)malloc(sizeof(B));
	this->b = "Allo";
	return this;
}
void B_DeConstruct(B* this) {
	free(this);
}
void B_Print(B* this) {
	printf("Hello World {");
	printf("%s", this->b);
	printf("}\n");
}
typedef struct {
	int a;
	char* id;
}C;
C* C_Construct(char* id) {
	C* this = (C*)malloc(sizeof(C));
	{
		A* temp = A_Construct();
		memcpy(this, temp, sizeof(A));
		free(temp);
	}
	this->id = id;
	return this;
}
C* C_Construct2(int a, char* id) {
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
