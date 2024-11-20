<?php
return [
    'allowed_mimes' => explode(',', env('ALLOWED_MIMES', 'jpeg,jpg,png,gif')),
    'allowed_sizes' => array_map('intval', explode(',', env('ALLOWED_SIZES', '144,240,360,480,720,1080'))),
    'warning_limit_for_ban' => env('WARNING_LIMIT_FOR_BAN', 3),
    'storage_size' => env('STORAGE_SIZE', 500_000_000), // В байтах
    'max_server_storage_size' => env('MAX_SERVER_STORAGE_SIZE', 1_000_000_000_000), //1 ТБ
];
