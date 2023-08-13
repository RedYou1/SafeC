enum Colors:
	blue
	red
	yellow
	green

class Container<T>:
	T a

	Container<V>(T a, *V b):
		this.a = a
		this.test<V>(b)

	void test<V>(this, V b):
		print("Container{")
		print(this.a)
		print(", ")
		print(b)
		print("}")

i32 main():
	new<Colors> Container<i32>(32i, Colors.blue)
	print("\n")
	new<i32> Container<Colors>(Colors.red, 16i)
	print("\n")
	new<str> Container<str>("A", "B")
	return 0i