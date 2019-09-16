using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace delimiterMMTD
{
    // C# program to implement a queue using an array  
    public class FixedSizedQueueStr
    {
        private static int front, rear, capacity;
        private static String[] queue;

        public FixedSizedQueueStr(int c)
        {
            front = rear = 0;
            capacity = c;
            queue = new String[capacity];
        }

        // function to insert an element  
        // at the rear of the queue  
        public void Enqueue(String data)
        {
            // check queue is full or not  
            if (capacity == rear)
            {
                Dequeue();
                Enqueue(data);
                return;
            }

            // insert element at the rear  
            else
            {
                queue[rear] = data;
                rear++;
            }
            return;
        }

        // function to delete an element  
        // from the front of the queue  
        public void Dequeue()
        {
            // if queue is empty  
            if (front == rear)
            {
                Console.Write("\nQueue is empty\n");
                return;
            }

            // shift all the elements from index 2 till rear  
            // to the right by one  
            else
            {
                for (int i = 0; i < rear - 1; i++)
                {
                    queue[i] = queue[i + 1];
                }

                // store 0 at rear indicating there's no element  
                if (rear < capacity)
                    queue[rear] = "0";

                // decrement rear  
                rear--;
            }
            return;
        }

        public void Shuffle()
        {
            int n = queue.Length;
            var rng = new Random();
            while (n > 1)
            {
                int k = rng.Next(n--);
                String temp = queue[n];
                queue[n] = queue[k];
                queue[k] = temp;
            }
        }

        public String Get(int index)
        {
            return queue[index];
        }

        public int Size()
        {
            int retVal;
            if (rear < capacity)
                retVal = rear;
            else
                retVal = capacity;
            return retVal;
        }

        // print queue elements  
        public void Display()
        {
            int i;
            if (front == rear)
            {
                Console.Write("\nQueue is Empty\n");
                return;
            }

            // traverse front to rear and print elements  
            for (i = front; i < rear; i++)
            {
                Console.Write(" {0} <-- ", queue[i]);
            }
            return;
        }

        // print front of queue  
        public void Front()
        {
            if (front == rear)
            {
                Console.Write("\nQueue is Empty\n");
                return;
            }
            Console.Write("\nFront Element is: {0}", queue[front]);
            return;
        }
    }
}