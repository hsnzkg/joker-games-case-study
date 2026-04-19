using System;
using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.Currency;
using Project.Scripts.Observable;
using Project.Scripts.SessionManagement;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.Currency
{
    public sealed class CurrencyManager
    {
        public const int defaultStartingAmount = 10000;

        private static readonly Lazy<CurrencyManager> s_instance = new(() => new CurrencyManager());

        public static CurrencyManager Instance => s_instance.Value;

        private Observable<int> Amount { get; }

        private CurrencyManager()
        {
            int initialAmount = LoadInitialAmount();
            Amount = new Observable<int>(initialAmount);
        }

        public bool Has(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to check cannot be negative.");
            }

            return Amount.Value >= amount;
        }

        public int GetAmount()
        {
            return Amount.Value;
        }

        public int Add(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add cannot be negative.");
            }

            if (amount == 0)
            {
                return Amount.Value;
            }

            int newAmount = checked(Amount.Value + amount);
            SetAmountInternal(newAmount);
            return newAmount;
        }

        public bool Remove(int amount)
        {
            return TryRemove(amount, out _);
        }

        public bool TryRemove(int amount, out int remainingAmount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount to remove cannot be negative.");
            }

            if (!Has(amount))
            {
                remainingAmount = Amount.Value;
                return false;
            }

            remainingAmount = Amount.Value - amount;
            SetAmountInternal(remainingAmount);
            return true;
        }

        public int SetAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Currency amount cannot be negative.");
            }

            SetAmountInternal(amount);
            return Amount.Value;
        }

        public void Reset()
        {
            SetAmountInternal(0);
        }

        public void ReloadFromStorage()
        {
            int reloadedAmount = LoadInitialAmount();

            if (Amount.Value == reloadedAmount)
            {
                return;
            }

            SetAmountInternal(reloadedAmount);
        }

        private void SetAmountInternal(int newAmount)
        {
            int previousAmount = Amount.Value;
            if (previousAmount == newAmount)
            {
                return;
            }

            Amount.Value = newAmount;
            SaveCurrentAmount();
            EventBus<ECurrencyChange>.Raise(new ECurrencyChange(previousAmount, newAmount));
        }

        private static int LoadInitialAmount()
        {
            int amount = defaultStartingAmount;
            GameData gameData = null;

            if (DataSerializer.TryLoadGameData(out GameData loadedGameData) && loadedGameData != null)
            {
                gameData = loadedGameData;
                amount = gameData.GetCurrencyAmountOrDefault(defaultStartingAmount);
            }

            gameData ??= new GameData(amount);
            gameData.CurrencyAmount = amount;
            gameData.GetStatisticsOrDefault();
            DataSerializer.SaveGameData(gameData);
            return amount;
        }

        private void SaveCurrentAmount()
        {
            GameData gameData = null;

            if (DataSerializer.TryLoadGameData(out GameData loadedGameData) && loadedGameData != null)
            {
                gameData = loadedGameData;
                gameData.GetStatisticsOrDefault();
            }

            gameData ??= new GameData(Amount.Value);
            gameData.CurrencyAmount = Amount.Value;
            DataSerializer.SaveGameData(gameData);
        }
    }
}
