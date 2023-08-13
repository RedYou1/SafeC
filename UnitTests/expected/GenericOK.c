#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct INumber INumber;
typedef struct Container$i32 {
	int a;
}Container$i32;
typedef enum Colors {
	Colors$blue,
	Colors$red,
	Colors$yellow,
	Colors$green,
}Colors;
void Container$i32_test$Colors(Container$i32 this, Colors b) {
	printf("Container{");
	printf("%i", this.a);
	printf(", ");
	printf("%i", b);
	printf("}");
}
Container$i32 Container$i32_Container_0$Colors(int a, Colors* b) {
	Container$i32 this;
	this.a = a;
	Container$i32_test$Colors(this, *b);
	return this;
}
typedef struct Container$Colors {
	Colors a;
}Container$Colors;
void Container$Colors_test$i32(Container$Colors this, int b) {
	printf("Container{");
	printf("%i", this.a);
	printf(", ");
	printf("%i", b);
	printf("}");
}
Container$Colors Container$Colors_Container_0$i32(Colors a, int* b) {
	Container$Colors this;
	this.a = a;
	Container$Colors_test$i32(this, *b);
	return this;
}
typedef struct Container$str {
	char* a;
}Container$str;
void Container$str_test$str(Container$str this, char* b) {
	printf("Container{");
	printf(this.a);
	printf(", ");
	printf(b);
	printf("}");
}
Container$str Container$str_Container_0$str(char* a, char** b) {
	Container$str this;
	this.a = a;
	Container$str_test$str(this, *b);
	return this;
}
int main() {
	Colors _0 = Colors$blue;
	Container$i32_Container_0$Colors(32, &_0);
	printf("\n");
	int _1 = 16;
	Container$Colors_Container_0$i32(Colors$red, &_1);
	printf("\n");
	char* _2 = "B";
	Container$str_Container_0$str("A", &_2);
	return 0;
}
