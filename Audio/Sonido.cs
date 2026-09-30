using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris2D.Audio
{
    public static class Sonido
    {
        public enum TonoJuego
        {
            Rotar, Mover, Fijar, Linea, Nivel, GameOver
        }

        private const int Tasa = 22050;
        private static readonly object Bloqueo = new();
        private static System.Media.SoundPlayer? _jugadorActual;
        private static MemoryStream? _flujoActual;
        private static DateTime _terminaActual;
        private static int _prioridadActual;

        public static void Reproducir(TonoJuego tono)
        {
            byte[] wav = Contruirwav(tono);
            if (wav.Length == 0)
                return;

            lock (Bloqueo)
            {
                int prioridad = ObtenerPrioridad(tono);
                bool estaSonando = DateTime.UtcNow < _terminaActual;
                if (estaSonando && prioridad < _prioridadActual)
                    return;

                _jugadorActual?.Stop();
                _flujoActual?.Dispose();

                _flujoActual = new MemoryStream(wav);
                _jugadorActual = new System.Media.SoundPlayer(_flujoActual);
                _prioridadActual = prioridad;
                _terminaActual = DateTime.UtcNow + ObtenerDuracion(tono);
                _jugadorActual.Play();
            }
        }

        private static int ObtenerPrioridad(TonoJuego tono) => tono switch
        {
            TonoJuego.GameOver => 4,
            TonoJuego.Nivel => 3,
            TonoJuego.Linea => 3,
            TonoJuego.Fijar => 2,
            TonoJuego.Rotar => 1,
            _ => 0
        };

        private static TimeSpan ObtenerDuracion(TonoJuego tono) => tono switch
        {
            TonoJuego.GameOver => TimeSpan.FromSeconds(0.70),
            TonoJuego.Nivel => TimeSpan.FromSeconds(0.28),
            TonoJuego.Linea => TimeSpan.FromSeconds(0.18),
            TonoJuego.Fijar => TimeSpan.FromSeconds(0.10),
            _ => TimeSpan.FromSeconds(0.05)
        };

        private static void Detener()
        {
            lock (Bloqueo)
            {
                _jugadorActual?.Stop();
                _flujoActual?.Dispose();
                _jugadorActual = null;
                _flujoActual = null;
                _terminaActual = DateTime.MinValue;
                _prioridadActual = 0;
            }
        }

        public static byte[] Contruirwav(TonoJuego tono)
        {
            // Frecuencia y duracion del tono segun el tipo de evento.
            return tono switch
            {
                TonoJuego.Mover => CrearBarrido(980, 980, 0.05, 0.30),
                TonoJuego.Rotar => CrearBarrido(740, 740, 0.05, 0.30),
                TonoJuego.Fijar => CrearBarrido(330, 300, 0.10, 0.50),
                TonoJuego.Linea => CrearBarrido(660, 990, 0.18, 0.60),
                TonoJuego.Nivel => CrearBarrido(440, 880, 0.28, 0.60),
                TonoJuego.GameOver => CrearBarrido(420, 90, 0.70, 0.65),
                _ => Array.Empty<byte>()
            };
        }

        private static byte[] CrearBarrido(double fInicio, double fFin, double segundos, double volumen)
        {
            int n = (int)(Tasa * segundos);
            var muestras = new byte[n * 2]; // 2 bytes por muestra (16 bits)

            for (int i = 0; i < n; i++)
            {
                double t = i / (double)Tasa;
                double progreso = i / (double)Math.Max(1, n - 1);
                double frecuencia = fInicio + (fFin - fInicio) * progreso;
                double seno = Math.Sin(2 * Math.PI * frecuencia * t) * volumen;
                short pcm = (short)Math.Clamp((double)(short)(seno * short.MaxValue), short.MinValue, short.MaxValue);
                muestras[i * 2] = (byte)(pcm & 0xFF);
                muestras[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
            }

            return EnvolverEnWab(muestras);
        }

        private static byte[] EnvolverEnWab(byte[] pcm)
        {
            var wav = new byte[44 + pcm.Length];
            
            Array.Copy(Encoding.ASCII.GetBytes("RIFF"), 0, wav, 0, 4);
            BitConverter.GetBytes(36 + pcm.Length).CopyTo(wav, 4);
            Array.Copy(Encoding.ASCII.GetBytes("WAVE"), 0, wav, 8, 4);
            Array.Copy(Encoding.ASCII.GetBytes("fmt "), 0, wav, 12, 4);
            BitConverter.GetBytes(16).CopyTo(wav, 16); // Tamaño del subchunk de formato
            BitConverter.GetBytes((short)1).CopyTo(wav, 20); // Formato PCM
            BitConverter.GetBytes((short)1).CopyTo(wav, 22); // Número de canales(mono)
            BitConverter.GetBytes(Tasa).CopyTo(wav, 24); // Frecuencia de muestreo
            BitConverter.GetBytes(Tasa * 2).CopyTo(wav, 28); // Byte por segundo
            BitConverter.GetBytes((short)2).CopyTo(wav, 32); // Alineamiento de bloque
            BitConverter.GetBytes((short)16).CopyTo(wav, 34); // Bits por muestra
            Array.Copy(Encoding.ASCII.GetBytes("data"), 0, wav, 36, 4);
            BitConverter.GetBytes(pcm.Length).CopyTo(wav, 40); // Tamaño de los datos
            Array.Copy(pcm, 0, wav, 44, pcm.Length);

            return wav;
        }
    }
}