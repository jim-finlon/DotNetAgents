-- Initial Migration SQL Script
-- This script creates the database schema manually
-- Use this as a reference or run it directly if EF Core migrations are not being used

-- ============================================
-- EXTENSIONS
-- ============================================
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "vector";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ============================================
-- ENUMS
-- ============================================
CREATE TYPE subscription_tier AS ENUM ('Free', 'Basic', 'Premium', 'Enterprise');
CREATE TYPE guardian_role AS ENUM ('Primary', 'Secondary', 'Teacher');
CREATE TYPE subject AS ENUM ('Biology', 'Chemistry', 'Physics', 'EarthScience', 'Astronomy', 'EnvironmentalScience', 'Mathematics');
CREATE TYPE grade_band AS ENUM ('K2', 'G3_5', 'G6_8', 'G9_10', 'G11_12');
CREATE TYPE content_type AS ENUM ('Concept', 'Lesson', 'Assessment', 'Lab', 'Vocabulary', 'Review');
CREATE TYPE bloom_level AS ENUM ('Remember', 'Understand', 'Apply', 'Analyze', 'Evaluate', 'Create');
CREATE TYPE mastery_level AS ENUM ('NotStarted', 'Introduced', 'Developing', 'Proficient', 'Mastered');
CREATE TYPE assessment_type AS ENUM ('Diagnostic', 'Formative', 'Summative', 'Practice');
CREATE TYPE question_type AS ENUM ('MultipleChoice', 'MultipleSelect', 'FillBlank', 'ShortAnswer', 'Matching', 'Ordering', 'TrueFalse', 'ConstructedResponse');

-- ============================================
-- TABLES
-- ============================================
-- Note: Tables will be created by EF Core migrations
-- This file serves as documentation of the schema
