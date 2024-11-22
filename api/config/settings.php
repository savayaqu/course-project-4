<?php
return [
    'allowed_upload_mimes'  => explode(',', env('ALLOWED_UPLOAD_MIMES', 'jpeg,jpg,png,gif')),
    'allowed_preview_sizes' => array_map('intval',
        explode(',', env('ALLOWED_PREVIEW_SIZES', '144,240,360,480,720,1080'))
    ),
    'warning_limit_for_ban'     => (int) env('WARNING_LIMIT_FOR_BAN'    , 3),
    'free_storage_limit'        => (int) env('FREE_STORAGE_LIMIT'       , 1024 * 1024 * 1024 * 5), // 5 GiB
    'upload_disable_percentage' => (int) env('UPLOAD_DISABLE_PERCENTAGE', 90),
];
