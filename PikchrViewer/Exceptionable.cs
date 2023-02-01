using System;

namespace PikchrViewer
{
    /// <summary>
    /// Class which can contain either an object of type <typeparamref name="T"/> or an exception.
    /// </summary>
    /// <remarks>
    /// This class allows a method to return either a value (in the case of success), or an
    /// Exception (in the case of failure), without having to create a class specially.
    /// 
    /// Some types of operation may naturally fail from time to time, e.g. calling a web service.  
    /// If failure is to some extent 'expected' at times, then this can be expressed by using 
    /// <see cref="Exceptionable{T}"/> as the method return type.  The caller then knows that
    /// the method may return a result or an exception, and can handle either case.
    /// 
    /// This is more expressive than the method simply throwing an exception in the case of 
    /// failure, as the method signature does not tell the caller that the method might throw, and
    /// that they must wrap the call in a try/catch.  It also avoids misusing the exception
    /// mechanism as a way of returning a failure value.
    /// 
    /// This is a simple version of the 'result monad'.  See:
    /// https://medium.com/@huund/monadic-error-handling-1e2ce66e3810
    /// https://adambennett.dev/2020/05/the-result-monad/
    /// </remarks>
    public class Exceptionable<T>
    {
        public readonly T Value;
        public readonly bool HasValue;
        public readonly bool HasException;
        public readonly Exception Exception;

        /// <summary>
        /// Initialises an Exceptionable object with a value of type <typeparamref name="T"/>.
        /// </summary>
        public Exceptionable(T value)
        {
            this.HasValue = true;
            this.Value = value;
        }

        /// <summary>
        /// Initialises an Exceptionable object with an exception.
        /// </summary>
        public Exceptionable(Exception ex)
        {
            this.Exception = ex;
            this.HasException = true;
        }

        // Allows an Exceptionable<T> to be passed to a method that takes a T parameter:
        public static implicit operator T(Exceptionable<T> m)
        {
            if (!m.HasValue)
            {
                throw new InvalidOperationException("Exceptionable object must have a value");
            }
            else
            {
                return m.Value;
            }
        }

        // Allows a method with return type Exceptionable<T> to return a T:
        public static implicit operator Exceptionable<T>(T value) => new Exceptionable<T>(value);

        // Allows a method with return type Exceptionable<T> to return an Exception:
        public static implicit operator Exceptionable<T>(Exception ex) => new Exceptionable<T>(ex);

        // Allows comparison between an Exceptionable<T> and a T.
        public static bool operator ==(Exceptionable<T> x, T y) => x.HasValue && x.Value.Equals(y);
        public static bool operator !=(Exceptionable<T> x, T y) => !(x == y);

        // Allows comparison between a T and an Exceptionable<T>.
        public static bool operator ==(T x, Exceptionable<T> y) => (y == x);
        public static bool operator !=(T x, Exceptionable<T> y) => (y != x);

        // Allows comparison between two Exceptionable<T> objects by value.
        public static bool operator ==(Exceptionable<T> x, Exceptionable<T> y) => x.Equals(y);
        public static bool operator !=(Exceptionable<T> x, Exceptionable<T> y) => !(x == y);

        public override bool Equals(object obj)
        {
            return
                obj is Exceptionable<T> other &&
                this.HasValue &&
                other.HasValue &&
                this.Value.Equals(other.Value);
        }

        public override int GetHashCode() => this.GetValueOrException().GetHashCode();
        public override string ToString() => this.GetValueOrException().ToString();

        private object GetValueOrException()
        {
            return this.HasValue ? this.Value : (object)this.Exception;
        }
    }
}