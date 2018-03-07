using System;
class Solution
{
    static void Main(string[] args)
    {
        int N = int.Parse(Console.ReadLine());
        string[] inputs = Console.ReadLine().Split(' ');
        int[] list = new int[N];
        for (int i = 0; i < N; i++)
        {
            list[i] = int.Parse(inputs[i]);
        }
        for(int i = 0; i < list.Length; i++)
        {
            int result = list[i] ^ list[i + 1];
        }
    }
}