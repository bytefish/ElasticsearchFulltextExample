DO $$

BEGIN

-- Initial Data
INSERT INTO fts.user(user_id, email, preferred_name, last_edited_by) 
    VALUES 
        (1, 'philipp@bytefish.de', 'Data Conversion User', 1)        
    ON CONFLICT DO NOTHING;

INSERT INTO fts.job_status(job_status_id, name, description, last_edited_by) 
    VALUES 
        (1, 'Scheduled', 'Job is scheduled for execution', 1), 
        (2, 'Executing', 'Job is executing', 1),
        (3, 'Paused', 'Job is paused', 1),
        (4, 'Finished', 'Job has been finished', 1), 
        (5, 'Failed', 'Job has failed', 1),
        (6, 'Cancelled', 'Job has been cancelled', 1)
    ON CONFLICT DO NOTHING;


END;
$$ LANGUAGE plpgsql;
