enum Colors:
	blue
	red
	yellow
	green

Container<T>:
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
	new<Colors> Container<i32>(32, Colors.blue)
	print("\n")
	new<i32> Container<Colors>(Colors.red, 16)
	return 0