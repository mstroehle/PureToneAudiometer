namespace Option
{
	using System;
	using System.Collections.Generic;

	/// <summary>
    /// Option type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Option<T>
    {
        private readonly T value;
        private readonly bool hasValue;
        /// <summary>
        /// Indicates whether the option holds a value or not
        /// </summary>
        public bool HasValue
        {
            get { return hasValue; }
        }

        private Option(T value)
        {
            this.value = value;
            hasValue = true;
        }

        /// <summary>
        /// Unwraps the option.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns>Specified, default value if the option is empty, or the option's value if present</returns>
        /// <remarks>It's recommended to use the <see cref="GetOrElse(Func{T})"/> when passing a parameter expression to be evaluated.
        /// In this case, the condition will get evaluated AFTER evaluation of the parameter, which may be costly.</remarks>
        public T GetOrElse(T defaultValue)
        {
            return HasValue ? value : defaultValue;
        }

        /// <summary>
        /// Unwraps the option
        /// </summary>
        /// <param name="defaultValue">Function returning the default value</param>
        /// <returns>The evaluation of the specified function if option is empty, or the option's value if present</returns>
        /// <remarks>Recommended overload. The function returning defaultValue will only get evaluated if
        /// the option is empty.</remarks>
        public T GetOrElse(Func<T> defaultValue)
        {
            return HasValue ? value : defaultValue();
        }

        /// <summary>
        /// Unwraps the option
        /// </summary>
        /// <returns>The default value for the type if option is empty, or the option's value if present</returns>
        public T GetOrDefault()
        {
            return GetOrElse(default(T));
        }

        public override string ToString()
        {
            return !HasValue ? "None" : value.ToString();
        }

		/// <summary>
		/// Empty option - None value
		/// </summary>
        public static readonly Option<T> None = new Option<T>();

		/// <summary>
		/// Creates an option holding a value.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when the argument is null. If there's no guarantee that the argument will have a value, use <see cref="From"/> instead</exception>
		/// <param name="value"></param>
		/// <returns></returns>
        public static Option<T> Some(T value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return new Option<T>(value);
        }

		/// <summary>
		/// Creates an option
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Either None or Some, depending if the value was null or not</returns>
        public static Option<T> From(T value)
        {
            if (value == null)
                return None;

            return Some(value);
        }

        /// <summary>
        /// Applies the specified function to option's value (if the option is non-empty) and yields a new option.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns>A new option after applying the function to current option's value if the current option is non-empty, <see cref="Option{T}.None"/> otherwise.</returns>
        public Option<TResult> Select<TResult>(Func<T, TResult> func)
        {
            if (!HasValue)
                return Option<TResult>.None;

            return Option<TResult>.From(func(value));
        }

        /// <summary>
        /// Applies a specified function to the option's value and yields a new option if the option is non-empty.
        /// <remarks>Different from <see cref="Select{U}"/>, expects a function that returns an <see cref="Option{T}"/></remarks>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns>A new option if the current option is non-empty, <see cref="Option{T}.None"/> otherwise.</returns>
        public Option<TResult> SelectMany<TResult>(Func<T, Option<TResult>> func)
        {
            if (!HasValue)
                return Option<TResult>.None;

            return func(value);
        }

        /// <summary>
        /// Filters the option by the passed predicate function
        /// </summary>
        /// <param name="func"></param>
        /// <returns>Returns the option if the option is non-empty and the value underneath satisfied the predicate, <see cref="Option{T}.None"/>  otherwise.</returns>
        public Option<T> Where(Func<T, bool> func)
        {
            if (HasValue && func(value))
                return this;

            return None;
        }

        /// <summary>
        /// Checks if the value 'exists' inside the option.
        /// </summary>
        /// <param name="func"></param>
        /// <returns><code>true</code> if the option is not empty and if it satisfied the predicate, <see cref="Option{T}.None"/> otherwise.</returns>
        public bool Any(Func<T, bool> func)
        {
            return HasValue && func(value);
        }

        /// <summary>
        /// Executes a specified action on the option, if the option is non-empty.
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            if (HasValue)
                action(value);
        }

        /// <summary>
        /// Returns the <paramref name="alternative"/> if the current option is empty. Returns the option itself otherwise.
        /// </summary>
        /// <param name="alternative"></param>
        /// <remarks>It's recommended to use <see cref="OrElse(Func{Option{T}})"/> overload.</remarks>
        /// <returns></returns>
        public Option<T> OrElse(Option<T> alternative)
        {
            return !HasValue ? alternative : this;
        }

        /// <summary>
        /// Checks whether the current option is empty; if it is, the <paramref name="alternative"/> function is evaluated and the result is returned. Otherwise,
        /// the calling instance is returned.
        /// </summary>
        /// <param name="alternative"></param>
        /// <returns>The current option if it's non-empty and the evaluation result of the alternative function otherwise.</returns>
        public Option<T> OrElse(Func<Option<T>> alternative)
        {
            return !HasValue ? alternative() : this;
        }

        /// <summary>
        /// Converts the option to a sequence (<see cref="IEnumerable{T}"/>)
        /// </summary>
        /// <returns>One element sequence containing the option's value if the option was non-empty, empty sequence otherwise</returns>
        public IEnumerable<T> ToEnumerable()
        {
            if (!HasValue) yield break;

            yield return value;
        }

		/// <summary>
		/// Implicit conversion from value to according option
		/// </summary>
		/// <param name="value">A value</param>
		/// <returns>Empty <c>Option</c> if value is null, non-emtpy <c>Option</c> otherwise</returns>
        public static implicit operator Option<T>(T value)
        {
            return From(value);
        }

    }

	/// <summary>
	/// Convenience and extension methods container
	/// </summary>
	public static class Option
	{
		/// <summary>
		/// Creates an option holding a value.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when the argument is null. If there's no guarantee that the argument will have a value, use <see cref="From{T}"/> instead</exception>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Option<T> Some<T>(T value)
		{
			return Option<T>.Some(value);
		}

		/// <summary>
		/// Creates an option
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Either None or Some, depending if the value was null or not</returns>
		public static Option<T> From<T>(T value)
		{
			return Option<T>.From(value);
		}

		/// <summary>
		/// Wraps the specified object in an option. If the object is null, returns <see cref="Option{T}.None"/>, otherwise creates a new option
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns>A new <see cref="Option{T}"/></returns>
		public static Option<T> ToOption<T>(this T value)
		{
			return From(value);
		}
	}
}
