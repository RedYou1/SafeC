Lock:
	&i32 a

	Lock(&i32 a):
		this.a = a

i32 main():
	i32 a = 5
	Lock lock = new Lock(a)
	Lock lock2 = new Lock(lock.a)
	return 0