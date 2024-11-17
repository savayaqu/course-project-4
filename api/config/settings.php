<?php
return [
    'allowed_mimes' => explode(',', env('ALLOWED_MIMES', 'jpeg,jpg,png,gif')),
    'allowed_sizes' => array_map('intval', explode(',', env('ALLOWED_SIZES', '144,240,360,480,720,1080'))),
    'warning_limit_for_ban' => env('WARNING_LIMIT_FOR_BAN', 3),
    'storage_size' => env('STORAGE_SIZE', 500000000), // (500 МБ)
    //'storage_size' => env('STORAGE_SIZE', 10000000), // (10 МБ для теста)
];
