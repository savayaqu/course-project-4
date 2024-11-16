<?php

namespace App\Exceptions\Api;

class ForbiddenException extends ApiException
{
    public function __construct($class = null)
    {
        $message = 'Forbidden' . ($class ? ' to ' . class_basename($class) : '');
        parent::__construct($message, 403);
    }
}
