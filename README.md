# Microphone-audio-level-tracker
Выполняется выключение компьютера после преодоления порогового значения уровня громкости

### Ключевой код

***Импортируем NAudio:***
```c#
using NAudio.Wave; // Устанавливается с помощью NuGet
``` 

***Постоянно прослушиваем микрофон:***

```c#
int RATE=44100; // частота микрофона
int BUFFER_SAMPLES=1024; // Степень двойки лучше работает с БПФ

var waveIn = new WaveInEvent();
waveIn.DeviceNumber = 0; // Измените это значения для разных девайсов
waveIn.WaveFormat = new NAudio.Wave.WaveFormat(RATE, 1); // 1 для моно входа
waveIn.DataAvailable += OnDataAvailable;
waveIn.BufferMilliseconds = (int)((double)BUFFER_SAMPLES / (double)RATE * 1000.0);
waveIn.StartRecording();
```

***Когда новый буфер заполнен данными, вызывается следующий код:***
```c#
private void OnDataAvailable(object sender, WaveInEventArgs args)
{
	// Показывает максимальное значения в буфере
    float max = 0;

    // перевод в 16-битный звук
    for (int index = 0; index < args.BytesRecorded; index += 2)
    {
        short sample = (short)((args.Buffer[index + 1] << 8) |
                                args.Buffer[index + 0]);
        var sample32 = sample / 32768f; 
        if (sample32 < 0) sample32 = -sample32; // Модуль значения 
        if (sample32 > max) max = sample32; // Это максимальное значение?
    }

    // Вычисление максимального значения по сравнению с остальными значениями
    if (max > audioValueMax)
    {
        audioValueMax = (double)max;
    }
    audioValueLast = max;
}
```

***Каждые [UPDATE_MILLISECONDS] миллисекунд вызывается timer_Tick:***
***Если уровень громкости превышает установленное процентное значение, ПК можно выключить***
```c#
private static void timer_Tick(object sender)
{
    double frac = audioValueLast / audioValueMax;

    if (frac * 100.0 > SHUTDOWN_PEAK)
    {
        Console.WriteLine("Shut down the computer");
        //Process.Start("shutdown", "-s -t 0");
    }
}
```

### Полезные ссылки
* [Оригинальный проект](https://github.com/swharden/Csharp-Data-Visualization/tree/master/projects/18-01-09_microphone_level_meter)
* [страница NAudio GitHub](https://github.com/naudio/NAudio)
* [NAudio Recording Level Meter (док)](https://github.com/naudio/NAudio/blob/master/Docs/RecordingLevelMeter.md)
* [NAudio Recording a WAV (док)](https://github.com/naudio/NAudio/blob/master/Docs/RecordWavFileWinFormsWaveIn.md)
