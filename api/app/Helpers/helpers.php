<?php
if (!function_exists('bytesToHuman')) {
    function bytesToHuman(int $bytes): string
    {
        $units = ['B', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB'];

        $bytes = max($bytes, 0);

        $pow = floor(($bytes ? log($bytes) : 0) / log(1024));
        $pow = min($pow, count($units) - 1);

        $bytes /= (1 << (10 * $pow));

        $formatted = number_format($bytes, 3 - strlen(floor($bytes)), '.', '');

        return $formatted . ' ' . $units[$pow];
    }
}
