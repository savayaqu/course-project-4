<?php

namespace App\Exceptions\Api;

class ForbiddenException extends ApiException
{
    public function __construct($message = null,  $class = null)
    {
        $message ??= 'Forbidden' . ($class ? ' to ' . class_basename($class) : '');
        parent::__construct($message, 403);
    }
}
