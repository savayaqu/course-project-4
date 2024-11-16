<?php

namespace App\Exceptions\Api;

class NotFoundException extends ApiException
{
    public function __construct($class = null)
    {
        $message = ucfirst(($class ? class_basename($class) . ' ' : '') . 'not found');
        parent::__construct($message, 404);
    }
}
