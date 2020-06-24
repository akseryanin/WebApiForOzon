using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.ExchnageRateDb
{
    // Модель для БД. Курс валюты к 1-ой кроне
    public class ExchangeRate{
        [Key]
        public long Id { get; set; }
        
        public DateTime Date { get; set; }
        
        public string Code { get; set; }
        
        public decimal Rate { get; set; }
    }
}