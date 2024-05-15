namespace UpdaterLibrary.Tools;

public static class ReflectionHelper
{
    public static object InvokeMethod(Type type, string methodName, object[] parameters)
    {
        // Получаем все методы с заданным именем
        var methods = type.GetMethods().Where(m => m.Name == methodName).ToArray();

        foreach (var method in methods)
        {
            // Получаем параметры метода
            var methodParameters = method.GetParameters();

            // Проверяем, что количество параметров метода не меньше, чем в переданном массиве
            if (methodParameters.Length < parameters.Length)
                continue;

            // Проверяем, совпадают ли типы первых параметров
            bool parametersMatch = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                // Если типы не совпадают и не совпадают с Nullable версией типа
                if (!methodParameters[i].ParameterType.IsAssignableFrom(parameters[i].GetType()) &&
                    !(Nullable.GetUnderlyingType(methodParameters[i].ParameterType)?.IsAssignableFrom(parameters[i].GetType()) ?? false))
                {
                    parametersMatch = false;
                    break;
                }
            }

            if (parametersMatch)
            {
                // Создаём массив параметров для вызова, заполняем его начальными значениями и Type.Missing для недостающих
                var invokeParameters = new object[methodParameters.Length];
                for (int i = 0; i < invokeParameters.Length; i++)
                {
                    if (i < parameters.Length)
                    {
                        invokeParameters[i] = parameters[i];
                    }
                    else
                    {
                        invokeParameters[i] = Type.Missing;
                    }
                }

                // Вызываем метод
                return method.Invoke(null, invokeParameters);
            }
        }

        throw new InvalidOperationException("No suitable method found.");
    }
}