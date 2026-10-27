using System;
using System.Collections.Generic;
using System.Text;

namespace EveryDayHub.Services
{
    // Generisk klass med platshållare som berättar att T måste vara en klass
    public class DataStore<T> where T : class
    {
        private readonly List<T> _items = new List<T>();
        private readonly string _filePath;

        // Konstruktor
        public DataStore(string filePath)
        {
            _filePath = filePath;
        }

        // Lägger till ett objekt i listan
        public void AddItem(T item)
        {
            _items.Add(item);
        }

        // Ta bort ett objekt och returnerar false om objektet saknas
        public bool RemoveItem(T item)
        {
            return _items.Remove(item);
        }

        // Söker efter objekt och returnerar första matchningen och returnerar null om inget matchar
        public T? FindItem(Predicate<T> match)
        {
            return _items.Find(match);
        }

        // Returnerar en kopia av listan
        public List<T> GetAll()
        {
            return new List<T>(_items);
        }
    }
}
